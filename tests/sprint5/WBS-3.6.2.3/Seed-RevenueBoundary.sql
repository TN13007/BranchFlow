-- Local Test only. Static aggregate fixtures, not an order-creation workflow.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME()<>N'BranchFlow_Test' THROW 51023,'Only BranchFlow_Test is allowed.',1;
BEGIN TRANSACTION;
DECLARE @a int=(SELECT Id FROM dbo.Branch WHERE Code=N'BF-DEMO-01'),
        @b int=(SELECT Id FROM dbo.Branch WHERE Code=N'BF-DEMO-02'),
        @actor int=(SELECT Id FROM dbo.AppUser WHERE Username=N'demo.employee.a'),
        @actorB int=(SELECT Id FROM dbo.AppUser WHERE Username=N'demo.employee.b');
IF @a IS NULL OR @b IS NULL OR @actor IS NULL OR @actorB IS NULL THROW 51023,'Requires existing demo branches and users.',1;
DECLARE @fixtures table(Code nvarchar(50),BranchId int,CreatedAt datetime2,Amount decimal(18,2),Status nvarchar(20));
INSERT @fixtures VALUES
 (N'WBS3623-BEFORE',@a,'2026-11-01T16:59:59',100000,N'COMPLETED'),
 (N'WBS3623-MIDNIGHT',@a,'2026-11-01T17:00:00',200000,N'COMPLETED'),
 (N'WBS3623-CANCELLED',@a,'2026-11-01T17:30:00',999000,N'CANCELLED'),
 (N'WBS3623-BRANCH-B',@b,'2026-11-01T18:00:00',300000,N'COMPLETED');
IF EXISTS(SELECT 1 FROM @fixtures f JOIN dbo.SalesOrder o ON o.Code=f.Code
 WHERE o.BranchId<>f.BranchId OR o.CreatedAt<>f.CreatedAt OR o.TotalAmount<>f.Amount OR o.Status<>f.Status)
 THROW 51023,'Existing fixture changed: inspect instead of overwriting.',1;
DECLARE @code nvarchar(50),@branch int,@utc datetime2,@amount decimal(18,2),@status nvarchar(20),@day date,@seq int;
DECLARE cases CURSOR LOCAL FAST_FORWARD FOR SELECT Code,BranchId,CreatedAt,Amount,Status FROM @fixtures;
OPEN cases;
FETCH NEXT FROM cases INTO @code,@branch,@utc,@amount,@status;
WHILE @@FETCH_STATUS=0
BEGIN
 IF NOT EXISTS(SELECT 1 FROM dbo.SalesOrder WHERE Code=@code)
 BEGIN
  SET @day=CONVERT(date,DATEADD(hour,7,@utc));
  SELECT @seq=COALESCE(MAX(DailySequence),0)+1 FROM dbo.SalesOrder WITH(UPDLOCK,HOLDLOCK) WHERE BranchId=@branch AND BusinessDate=@day;
  INSERT dbo.SalesOrder(Code,BranchId,CreatedByUserId,BusinessDate,DailySequence,TotalAmount,Status,CreatedAt)
  VALUES(@code,@branch,CASE WHEN @branch=@b THEN @actorB ELSE @actor END,@day,@seq,@amount,@status,@utc);
 END;
 FETCH NEXT FROM cases INTO @code,@branch,@utc,@amount,@status;
END;
CLOSE cases; DEALLOCATE cases;
COMMIT;
SELECT Id,Code,BranchId,CreatedAt AS UtcTime,DATEADD(hour,7,CreatedAt) AS VietnamTime,Status,TotalAmount
 FROM dbo.SalesOrder WHERE Code LIKE N'WBS3623-%' ORDER BY Code;
IF (SELECT COUNT(*) FROM dbo.SalesOrder WHERE Code LIKE N'WBS3623-%')<>4 THROW 51023,'Expected four fixture headers.',1;
PRINT 'PASS four static revenue fixtures; rerun preserves existing records';
