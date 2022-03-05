using Logistics.Blazor.Pagination;
using NUnit.Framework;

namespace Logistics.Blazor.Tests;

public class PagedListTest
{
    [Test]
    public void AddItem()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        pagedList.Add("1");
        pagedList.Add("2");
        pagedList.Add("3");
        pagedList.Add("3");
        pagedList.Add("4");

        Assert.AreEqual(4, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
        Assert.AreEqual(1, pagedList.PagesCount);
        Assert.AreEqual("4", pagedList[3]);
    }

    [Test]
    public void AddRange()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        var nums = new[] { "1", "2", "3", "3", "4" };
        pagedList.AddRange(nums);

        Assert.AreEqual(4, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
        Assert.AreEqual(1, pagedList.PagesCount);
        Assert.AreEqual("4", pagedList[3]);
    }

    [Test]
    public void Insert()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        pagedList.Add("1");
        pagedList.Add("2");
        pagedList.Add("3");
        pagedList.Add("4");
        pagedList.Add("5");

        pagedList.Insert(0, "11");
        pagedList.Insert(1, "12");
        pagedList.Insert(2, "13");
        pagedList.Insert(3, "13");
        pagedList.Insert(4, "14");

        Assert.AreEqual(9, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
        Assert.AreEqual(1, pagedList.PagesCount);
        Assert.AreEqual("14", pagedList[4]);
    }

    [Test]
    public void InsertRange()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        var nums1 = new[] { "1", "2", "3" };
        var nums2 = new[] { "4", "4", "5", "6" };
        pagedList.AddRange(nums1);
        pagedList.InsertRange(0, nums2);

        Assert.AreEqual(6, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
        Assert.AreEqual(1, pagedList.PagesCount);
        Assert.AreEqual("5", pagedList[2]);
    }

    [Test]
    public void Remove()
    {
        var pagedList = new PagedList<string>(5, 10, true, i => i);
        pagedList.Add("1");
        pagedList.Add("2");
        pagedList.Add("3");
        pagedList.Add("4");
        pagedList.Add("5");
        pagedList.Remove("2");

        Assert.AreEqual(4, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
        Assert.AreEqual(1, pagedList.PagesCount);
        Assert.AreEqual("1", pagedList[0]);
        Assert.False(pagedList.Contains("2"));
    }

    [Test]
    public void Clear()
    {
        var pagedList = new PagedList<string>(5);
        pagedList.Add("1");
        pagedList.Add("2");
        pagedList.Add("3");
        pagedList.Add("4");
        pagedList.Add("5");
        pagedList.Clear();

        Assert.AreEqual(0, pagedList.Count);
        Assert.AreEqual(5, pagedList.TotalItems);
    }
}