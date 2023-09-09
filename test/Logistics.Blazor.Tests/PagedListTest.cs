using Logistics.BlazorComponents.Pagination;

namespace Logistics.Blazor.Tests;

public class PagedListTest
{
    [Fact]
    public void AddItem()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i)
        {
            "1", "2", "3", "3", "4"
        };

        Assert.Equal(4, pagedList.Count);
        Assert.Equal(5, pagedList.TotalItems);
        Assert.Equal(1, pagedList.PagesCount);
        Assert.Equal("4", pagedList[3]);
    }

    [Fact]
    public void AddRange()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        var nums = new[] { "1", "2", "3", "3", "4" };
        pagedList.AddRange(nums);

        Assert.Equal(4, pagedList.Count);
        Assert.Equal(5, pagedList.TotalItems);
        Assert.Equal(1, pagedList.PagesCount);
        Assert.Equal("4", pagedList[3]);
    }

    [Fact]
    public void Insert()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i)
        {
            "1", "2", "3", "4", "5"
        };

        pagedList.Insert(0, "11");
        pagedList.Insert(1, "12");
        pagedList.Insert(2, "13");
        pagedList.Insert(3, "13");
        pagedList.Insert(4, "14");

        Assert.Equal(9, pagedList.Count);
        Assert.Equal(5, pagedList.TotalItems);
        Assert.Equal(1, pagedList.PagesCount);
        Assert.Equal("14", pagedList[4]);
    }

    [Fact]
    public void InsertRange()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        var nums1 = new[] { "1", "2", "3" };
        var nums2 = new[] { "4", "4", "5", "6" };
        pagedList.AddRange(nums1);
        pagedList.InsertRange(0, nums2);

        Assert.Equal(6, pagedList.Count);
        Assert.Equal(5, pagedList.TotalItems);
        Assert.Equal(1, pagedList.PagesCount);
        Assert.Equal("5", pagedList[2]);
    }

    [Fact]
    public void Remove()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i)
        {
            "1", "2", "3", "4", "5"
        };
        pagedList.Remove("2");

        Assert.Equal(4, pagedList.Count);
        Assert.Equal(5, pagedList.TotalItems);
        Assert.Equal(1, pagedList.PagesCount);
        Assert.Equal("1", pagedList[0]);
        Assert.DoesNotContain("2", pagedList);
    }

    [Fact]
    public void Clear()
    {
        var pagedList = new PagedList<string>(5, 10)
        {
            "1", "2", "3", "4", "5"
        };
        pagedList.Clear();

        Assert.Empty(pagedList);
        Assert.Equal(5, pagedList.TotalItems);
    }
}
