using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;

namespace BranchFlow.Application.Services;

public sealed class SalesDemoSeedService : ISalesDemoSeedService
{
    private const string Branch01Code = "BF-DEMO-01";
    private const string Branch02Code = "BF-DEMO-02";
    private const string Branch03Code = "BF-DEMO-03";

    private readonly ISalesDemoSeedRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SalesDemoSeedService(
        ISalesDemoSeedRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            DateTime now = DateTime.UtcNow;

            // 1. Lấy các chi nhánh đã được Auth/Branch Seed tạo.
            List<Branch> branches = await _repository.GetBranchesByCodesAsync(
                [Branch01Code, Branch02Code, Branch03Code],
                cancellationToken);

            Dictionary<string, Branch> branchMap = branches.ToDictionary(
                x => x.Code,
                StringComparer.Ordinal);

            foreach (string branchCode in new[]
                     {
                         Branch01Code,
                         Branch02Code,
                         Branch03Code
                     })
            {
                if (!branchMap.TryGetValue(branchCode, out Branch? branch) ||
                    branch.Deleted ||
                    !branch.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Không tìm thấy chi nhánh demo đang hoạt động: {branchCode}. Hãy chạy Auth/Branch Seed trước.");
                }
            }

            // 2. Tạo Category.
            List<Category> categories = await _repository.GetCategoriesByNamesAsync(
                ["Demo - Cà phê", "Demo - Trà"],
                cancellationToken);

            Dictionary<string, Category> categoryMap = categories.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            EnsureCategory(categoryMap, "Demo - Cà phê", now);
            EnsureCategory(categoryMap, "Demo - Trà", now);

            _repository.AddCategories(
                categoryMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Tạo Product.
            List<Product> products = await _repository.GetProductsByNamesAsync(
                [
                    "Demo - Cà phê sữa",
                    "Demo - Trà đào",
                    "Demo - Matcha latte",
                    "Demo - Món ngừng bán"
                ],
                cancellationToken);

            Dictionary<string, Product> productMap = products.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            EnsureProduct(
                productMap,
                "Demo - Cà phê sữa",
                categoryMap["Demo - Cà phê"].Id,
                true,
                now);

            EnsureProduct(
                productMap,
                "Demo - Trà đào",
                categoryMap["Demo - Trà"].Id,
                true,
                now);

            EnsureProduct(
                productMap,
                "Demo - Matcha latte",
                categoryMap["Demo - Trà"].Id,
                true,
                now);

            EnsureProduct(
                productMap,
                "Demo - Món ngừng bán",
                categoryMap["Demo - Cà phê"].Id,
                false,
                now);

            // Bổ sung ảnh cho món demo, giữ nguyên tên/giá và cách seed của thành viên.
            foreach (Product demoProduct in productMap.Values)
            {
                if (string.IsNullOrWhiteSpace(demoProduct.ImageUrl))
                {
                    demoProduct.ImageUrl = demoProduct.Name switch
                    {
                        "Demo - Cà phê sữa" => "/images/menu/coffee.png",
                        "Demo - Trà đào" => "/images/menu/tea.png",
                        "Demo - Matcha latte" => "/images/menu/matcha.png",
                        _ => null
                    };
                }
            }

            _repository.AddProducts(
                productMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Đảm bảo có Size M và L.
            List<Size> sizes = await _repository.GetSizesByNamesAsync(
                ["M", "L"],
                cancellationToken);

            Dictionary<string, Size> sizeMap = sizes.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            EnsureSize(sizeMap, "M", now);
            EnsureSize(sizeMap, "L", now);

            _repository.AddSizes(
                sizeMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Tạo ProductSize và giá cho từng Size.
            var productSizePrices = new[]
            {
                (Product: "Demo - Cà phê sữa", Size: "M", Price: 29000m),
                (Product: "Demo - Cà phê sữa", Size: "L", Price: 35000m),

                (Product: "Demo - Trà đào", Size: "M", Price: 32000m),
                (Product: "Demo - Trà đào", Size: "L", Price: 39000m),

                (Product: "Demo - Matcha latte", Size: "M", Price: 35000m),
                (Product: "Demo - Matcha latte", Size: "L", Price: 42000m),

                (Product: "Demo - Món ngừng bán", Size: "M", Price: 25000m),
                (Product: "Demo - Món ngừng bán", Size: "L", Price: 30000m)
            };

            List<int> productIds = productMap.Values.Select(x => x.Id).ToList();

            List<ProductSize> existingProductSizes =
                await _repository.GetProductSizesAsync(
                    productIds,
                    cancellationToken);

            foreach (var item in productSizePrices)
            {
                Product product = productMap[item.Product];
                Size size = sizeMap[item.Size];

                ProductSize? existing = existingProductSizes.FirstOrDefault(
                    x => x.ProductId == product.Id && x.SizeId == size.Id);

                if (existing is not null)
                {
                    if (!existing.IsActive || existing.Price != item.Price)
                    {
                        throw new InvalidOperationException(
                            $"ProductSize của '{item.Product}' - Size {item.Size} không khớp cấu hình demo.");
                    }

                    continue;
                }

                existingProductSizes.Add(new ProductSize
                {
                    ProductId = product.Id,
                    SizeId = size.Id,
                    Price = item.Price,
                    IsActive = true,
                    CreatedAt = now,
                    Deleted = false
                });
            }

            _repository.AddProductSizes(
                existingProductSizes.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Tạo ToppingGroup.
            List<ToppingGroup> toppingGroups =
                await _repository.GetToppingGroupsByNamesAsync(
                    ["Demo - Topping thêm"],
                    cancellationToken);

            Dictionary<string, ToppingGroup> toppingGroupMap =
                toppingGroups.ToDictionary(x => x.Name, StringComparer.Ordinal);

            EnsureToppingGroup(toppingGroupMap, "Demo - Topping thêm", now);

            _repository.AddToppingGroups(
                toppingGroupMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Tạo Topping.
            List<Topping> toppings = await _repository.GetToppingsByNamesAsync(
                [
                    "Demo - Trân châu đen",
                    "Demo - Thạch đào",
                    "Demo - Pudding"
                ],
                cancellationToken);

            Dictionary<string, Topping> toppingMap = toppings.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            int toppingGroupId = toppingGroupMap["Demo - Topping thêm"].Id;

            EnsureTopping(
                toppingMap,
                "Demo - Trân châu đen",
                toppingGroupId,
                5000m,
                now);

            EnsureTopping(
                toppingMap,
                "Demo - Thạch đào",
                toppingGroupId,
                7000m,
                now);

            EnsureTopping(
                toppingMap,
                "Demo - Pudding",
                toppingGroupId,
                8000m,
                now);

            _repository.AddToppings(
                toppingMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 8. Tạo NoteGroup.
            List<NoteGroup> noteGroups =
                await _repository.GetNoteGroupsByNamesAsync(
                    ["Demo - Đường", "Demo - Đá"],
                    cancellationToken);

            Dictionary<string, NoteGroup> noteGroupMap = noteGroups.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            EnsureNoteGroup(noteGroupMap, "Demo - Đường", now);
            EnsureNoteGroup(noteGroupMap, "Demo - Đá", now);

            _repository.AddNoteGroups(
                noteGroupMap.Values.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Tạo NoteOption.
            List<int> noteGroupIds = noteGroupMap.Values.Select(x => x.Id).ToList();

            List<NoteOption> noteOptions =
                await _repository.GetNoteOptionsAsync(
                    noteGroupIds,
                    cancellationToken);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đường"].Id,
                "100% đường",
                now);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đường"].Id,
                "50% đường",
                now);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đường"].Id,
                "0% đường",
                now);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đá"].Id,
                "Bình thường",
                now);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đá"].Id,
                "Ít đá",
                now);

            EnsureNoteOption(
                noteOptions,
                noteGroupMap["Demo - Đá"].Id,
                "Không đá",
                now);

            _repository.AddNoteOptions(
                noteOptions.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 10. Thiết lập khả dụng của Product theo chi nhánh.
            var branchProductAvailability = new[]
            {
                (Branch: Branch01Code, Product: "Demo - Cà phê sữa", Available: true),
                (Branch: Branch01Code, Product: "Demo - Trà đào", Available: true),
                (Branch: Branch01Code, Product: "Demo - Matcha latte", Available: true),
                (Branch: Branch01Code, Product: "Demo - Món ngừng bán", Available: false),

                (Branch: Branch02Code, Product: "Demo - Cà phê sữa", Available: true),
                (Branch: Branch02Code, Product: "Demo - Trà đào", Available: false),
                (Branch: Branch02Code, Product: "Demo - Matcha latte", Available: true),
                (Branch: Branch02Code, Product: "Demo - Món ngừng bán", Available: false),

                (Branch: Branch03Code, Product: "Demo - Cà phê sữa", Available: true),
                (Branch: Branch03Code, Product: "Demo - Trà đào", Available: true),
                (Branch: Branch03Code, Product: "Demo - Matcha latte", Available: false),
                (Branch: Branch03Code, Product: "Demo - Món ngừng bán", Available: false)
            };

            List<int> branchIds = branchMap.Values.Select(x => x.Id).ToList();

            List<BranchProduct> existingBranchProducts =
                await _repository.GetBranchProductsAsync(
                    branchIds,
                    productIds,
                    cancellationToken);

            foreach (var item in branchProductAvailability)
            {
                int branchId = branchMap[item.Branch].Id;
                int productId = productMap[item.Product].Id;

                BranchProduct? existing = existingBranchProducts.FirstOrDefault(
                    x => x.BranchId == branchId && x.ProductId == productId);

                if (existing is not null)
                {
                    if (existing.IsAvailable != item.Available)
                    {
                        throw new InvalidOperationException(
                            $"Trạng thái Product '{item.Product}' tại '{item.Branch}' khác cấu hình demo.");
                    }

                    continue;
                }

                existingBranchProducts.Add(new BranchProduct
                {
                    BranchId = branchId,
                    ProductId = productId,
                    IsAvailable = item.Available
                });
            }

            _repository.AddBranchProducts(
                existingBranchProducts.Where(x => x.Id == 0));

            // 11. Thiết lập khả dụng của Topping theo chi nhánh.
            var branchToppingAvailability = new[]
            {
                (Branch: Branch01Code, Topping: "Demo - Trân châu đen", Available: true),
                (Branch: Branch01Code, Topping: "Demo - Thạch đào", Available: true),
                (Branch: Branch01Code, Topping: "Demo - Pudding", Available: true),

                (Branch: Branch02Code, Topping: "Demo - Trân châu đen", Available: false),
                (Branch: Branch02Code, Topping: "Demo - Thạch đào", Available: true),
                (Branch: Branch02Code, Topping: "Demo - Pudding", Available: true),

                (Branch: Branch03Code, Topping: "Demo - Trân châu đen", Available: true),
                (Branch: Branch03Code, Topping: "Demo - Thạch đào", Available: true),
                (Branch: Branch03Code, Topping: "Demo - Pudding", Available: false)
            };

            List<int> toppingIds = toppingMap.Values.Select(x => x.Id).ToList();

            List<BranchTopping> existingBranchToppings =
                await _repository.GetBranchToppingsAsync(
                    branchIds,
                    toppingIds,
                    cancellationToken);

            foreach (var item in branchToppingAvailability)
            {
                int branchId = branchMap[item.Branch].Id;
                int toppingId = toppingMap[item.Topping].Id;

                BranchTopping? existing = existingBranchToppings.FirstOrDefault(
                    x => x.BranchId == branchId && x.ToppingId == toppingId);

                if (existing is not null)
                {
                    if (existing.IsAvailable != item.Available)
                    {
                        throw new InvalidOperationException(
                            $"Trạng thái Topping '{item.Topping}' tại '{item.Branch}' khác cấu hình demo.");
                    }

                    continue;
                }

                existingBranchToppings.Add(new BranchTopping
                {
                    BranchId = branchId,
                    ToppingId = toppingId,
                    IsAvailable = item.Available,
                    UpdatedAt = now
                });
            }

            _repository.AddBranchToppings(
                existingBranchToppings.Where(x => x.Id == 0));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            }
            catch
            {
                // Giữ nguyên exception gốc.
            }

            throw;
        }
    }

    private static void EnsureCategory(
        IDictionary<string, Category> categories,
        string name,
        DateTime now)
    {
        if (categories.TryGetValue(name, out Category? existing))
        {
            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"Category '{name}' không ở trạng thái hoạt động.");
            }

            return;
        }

        categories[name] = new Category
        {
            Name = name,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureProduct(
        IDictionary<string, Product> products,
        string name,
        int categoryId,
        bool isActive,
        DateTime now)
    {
        if (products.TryGetValue(name, out Product? existing))
        {
            if (existing.CategoryId != categoryId ||
                existing.IsActive != isActive)
            {
                throw new InvalidOperationException(
                    $"Product '{name}' không khớp cấu hình demo.");
            }

            return;
        }

        products[name] = new Product
        {
            Name = name,
            CategoryId = categoryId,
            IsActive = isActive,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureSize(
        IDictionary<string, Size> sizes,
        string name,
        DateTime now)
    {
        if (sizes.TryGetValue(name, out Size? existing))
        {
            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"Size '{name}' không ở trạng thái hoạt động.");
            }

            return;
        }

        sizes[name] = new Size
        {
            Name = name,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureToppingGroup(
        IDictionary<string, ToppingGroup> groups,
        string name,
        DateTime now)
    {
        if (groups.TryGetValue(name, out ToppingGroup? existing))
        {
            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"ToppingGroup '{name}' không ở trạng thái hoạt động.");
            }

            return;
        }

        groups[name] = new ToppingGroup
        {
            Name = name,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureTopping(
        IDictionary<string, Topping> toppings,
        string name,
        int groupId,
        decimal price,
        DateTime now)
    {
        if (toppings.TryGetValue(name, out Topping? existing))
        {
            if (existing.ToppingGroupId != groupId ||
                existing.Price != price ||
                !existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"Topping '{name}' không khớp cấu hình demo.");
            }

            return;
        }

        toppings[name] = new Topping
        {
            Name = name,
            ToppingGroupId = groupId,
            Price = price,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureNoteGroup(
        IDictionary<string, NoteGroup> groups,
        string name,
        DateTime now)
    {
        if (groups.TryGetValue(name, out NoteGroup? existing))
        {
            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"NoteGroup '{name}' không ở trạng thái hoạt động.");
            }

            return;
        }

        groups[name] = new NoteGroup
        {
            Name = name,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        };
    }

    private static void EnsureNoteOption(
        ICollection<NoteOption> options,
        int groupId,
        string name,
        DateTime now)
    {
        NoteOption? existing = options.FirstOrDefault(
            x => x.NoteGroupId == groupId && x.Name == name);

        if (existing is not null)
        {
            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    $"NoteOption '{name}' không ở trạng thái hoạt động.");
            }

            return;
        }

        options.Add(new NoteOption
        {
            NoteGroupId = groupId,
            Name = name,
            IsActive = true,
            CreatedAt = now,
            Deleted = false
        });
    }
}
