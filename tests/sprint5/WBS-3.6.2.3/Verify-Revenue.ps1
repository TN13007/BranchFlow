param([string]$BaseUrl='https://localhost:17317')
$ErrorActionPreference='Stop'
if ($BaseUrl -notmatch '^https://localhost:\d+$') { throw 'Local HTTPS only.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Provide existing password in process env.' }
$sessions=@{}
$script:count=0
function Assert($condition,$label) {
 if (-not $condition) { throw "FAIL $label" }
 $script:count++; Write-Output "PASS $label"
}
function GetRows($role,$from,$to,$branch=0) {
 $url="$BaseUrl/api/management/revenue?from=$from&to=$to"
 if ($branch) { $url+="&branchId=$branch" }
 foreach($row in (Invoke-RestMethod $url -Headers @{Authorization="Bearer $($sessions[$role])"})) { $row }
}
function Denied($role,$path,$status,$method='Get',$inputObject=$null) {
 $headers=@{}
 if ($role) { $headers.Authorization="Bearer $($sessions[$role])" }
 try {
  $params=@{Uri="$BaseUrl/$path";Headers=$headers;Method=$method}
  if ($null -ne $inputObject) { $params.ContentType='application/json'; $params.Body=($inputObject|ConvertTo-Json) }
  Invoke-RestMethod @params | Out-Null
  throw "Expected $status but request succeeded"
 } catch {
  Assert ([int]$_.Exception.Response.StatusCode -eq $status) "$role $method $path returns $status"
 }
}
try {
 foreach($role in @('owner','systemadmin','branchadmin.01','employee.a')) {
  $body=@{username="demo.$role";password=$env:BRANCHFLOW_DEMO_PASSWORD}|ConvertTo-Json
  $pair=Invoke-RestMethod "$BaseUrl/api/auth/login" -Method Post -ContentType application/json -Body $body
  $sessions[$role]=$pair.accessToken
  Assert ([bool]$pair.accessToken) "$role authenticated with existing password"
 }
 $branches=Invoke-RestMethod "$BaseUrl/api/management/branches" -Headers @{Authorization="Bearer $($sessions.owner)"}
 $a=($branches|Where-Object code -eq 'BF-DEMO-01').id
 $b=($branches|Where-Object code -eq 'BF-DEMO-02').id
 Assert ($a -and $b) 'demo branches found'
 $sql=& sqlcmd -S localhost -E -C -b -d BranchFlow_Test -h -1 -w 65535 -y 4000 -Q "SET NOCOUNT ON; SELECT BranchId AS branchId,CONVERT(char(10),DATEADD(hour,7,CreatedAt),23) AS date,COUNT(*) AS orders,SUM(TotalAmount) AS amount FROM dbo.SalesOrder WHERE Status='COMPLETED' AND CreatedAt>='2026-10-31T17:00:00' AND CreatedAt<'2026-11-02T17:00:00' GROUP BY BranchId,CONVERT(char(10),DATEADD(hour,7,CreatedAt),23) FOR JSON PATH;"
 if ($LASTEXITCODE -ne 0) { throw 'SQL read failed' }
 $expected=@(ConvertFrom-Json ($sql -join ''))
 $owner=@(GetRows owner '2026-11-01' '2026-11-02')
 foreach($role in @('owner','systemadmin')) {
  $rows=@(GetRows $role '2026-11-01' '2026-11-02')
  Assert ($rows.Count -eq $expected.Count) "$role row count equals SQL"
  foreach($e in $expected) {
   $r=@($rows|Where-Object { $_.branchId -eq $e.branchId -and $_.date -eq $e.date })
   Assert ($r.Count -eq 1 -and $r[0].orders -eq $e.orders -and $r[0].amount -eq $e.amount) "$role branch $($e.branchId) day $($e.date) equals SQL: $($e.orders) orders $($e.amount) VND"
  }
 }
 $before=@(GetRows owner '2026-11-01' '2026-11-01' $a)
 $after=@(GetRows owner '2026-11-02' '2026-11-02' $a)
 Assert ($before.Count -eq 1 -and $before[0].amount -eq 100000 -and $before[0].orders -eq 1) '23:59:59 Vietnam belongs to Nov 1: 100000'
 Assert ($after.Count -eq 1 -and $after[0].amount -eq 200000 -and $after[0].orders -eq 1) '00:00:00 Vietnam belongs to Nov 2: 200000, cancelled 999000 excluded'
 $other=@(GetRows owner '2026-11-02' '2026-11-02' $b)
 Assert ($other.Count -eq 1 -and $other[0].amount -eq 300000) 'branch B Nov 2 equals 300000'
 Assert (($owner|Measure-Object amount -Sum).Sum -eq 600000) 'system total equals 600000 completed revenue'
 $admin=@(GetRows 'branchadmin.01' '2026-11-01' '2026-11-02')
 Assert (@($admin|Where-Object branchId -ne $a).Count -eq 0 -and $admin.Count -eq 2 -and ($admin|Measure-Object amount -Sum).Sum -eq 300000) 'branch admin implicit scope only A: 300000'
 $explicit=@(GetRows 'branchadmin.01' '2026-11-01' '2026-11-02' $a)
 Assert ($explicit.Count -eq $admin.Count -and ($explicit|Measure-Object amount -Sum).Sum -eq 300000) 'branch admin explicit A matches implicit scope'
 Denied 'branchadmin.01' "api/management/revenue?from=2026-11-01&to=2026-11-02&branchId=$b" 403
 Denied 'employee.a' 'api/management/revenue?from=2026-11-01&to=2026-11-02' 403
 Denied '' 'api/management/revenue?from=2026-11-01&to=2026-11-02' 401
 Denied owner 'api/management/revenue?from=2026-11-02&to=2026-11-01' 400
 Denied owner "api/management/branches/$a" 403 Put @{code='BF-DEMO-01';name='Not applied';address='Test';isActive=$true}
 Assert (@(GetRows owner '2001-01-01' '2001-01-01').Count -eq 0) 'valid empty date returns empty array'
 Write-Output "RESULT: $script:count assertions PASS; local HTTP and SQL, UI recorded separately."
} finally {
 foreach($token in $sessions.Values) {
  Invoke-RestMethod "$BaseUrl/api/auth/logout" -Method Post -Headers @{Authorization="Bearer $token"} | Out-Null
 }
}
