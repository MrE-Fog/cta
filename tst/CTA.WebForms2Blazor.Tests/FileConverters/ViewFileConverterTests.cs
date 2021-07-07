using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.FileConverters
{
    [TestFixture]
    public class ViewFileConverterTests
    {
        [Test]
        public async Task HyperLinkControlConverter_Returns_Href_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestHyperLinkControlFilePath);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "HyperLinkControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);

            string expectedContents = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <a class=""esh-pager-item esh-pager-item--navigable"">
                    Previous
                </a>

                <a class=""esh-pager-item esh-pager-item--navigable"">
                    Next
                </a>
            </nav>
            <td>
                <a href='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) )' class=""esh-table-link"">
                    Edit
                </a>
                |
                <a href='@(GetRouteUrl(""ProductDetailsRoute"", new {id =Item.Id}) )' class=""esh-table-link"">
                    Details
                </a>
                |
                <a href='@(GetRouteUrl(""DeleteProductRoute"", new {id =Item.Id}) )' class=""esh-table-link"">
                    Delete
                </a>
            </td>
        </article>
    </div>
</div>";

            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("href"));
            Assert.True(fileContents.Contains("class"));
            Assert.False(fileContents.Contains("asp:HyperLink"));
            Assert.False(fileContents.Contains("CssClass"));
            Assert.False(fileContents.Contains("NavigateUrl"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
        }

        [Test]
        public async Task ButtonControlConverter_Returns_Button_Node()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestButtonControlFilePath);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "ButtonControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);

            string expectedContents = @"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dd class=""text-right esh-button-actions"">
            <a runat=""server"" href=""~"" class=""btn esh-button esh-button-secondary"">[ Cancel ]
            </a>
            <button class=""btn esh-button esh-button-primary"" @onclick=""Delete_Click"">[ Delete ]</button>
            <button class=""btn test no-text"" @onclick=""Do_Something""></button>
        </dd>
    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("button"));
            Assert.True(fileContents.Contains("class"));
            Assert.True(fileContents.Contains("</button>"));
            Assert.False(fileContents.Contains("asp:Button"));
            Assert.False(fileContents.Contains("CssClass"));
            Assert.False(fileContents.Contains("Text"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
        }

        [Test]
        public async Task LabelControlConverter_Returns_DynamicText()
        {
            FileConverter fc = new ViewFileConverter(FileConverterSetupFixture.TestProjectPath, 
                FileConverterSetupFixture.TestLabelControlFilePath);
            
            IEnumerable<FileInformation> fileList = await fc.MigrateFileAsync();
            FileInformation fi = fileList.Single();
            
            byte[] bytes = fi.FileBytes;
            var fileContents = Encoding.UTF8.GetString(bytes);
            
            string newPath = Path.Combine(FileConverterSetupFixture.TestFilesDirectoryPath, "LabelControlOnly.razor");
            string relativePath = Path.GetRelativePath(FileConverterSetupFixture.TestProjectPath, newPath);

            string expectedContents = @"<div class=""row"">
    <dl class=""col-md-6 dl-horizontal"">
        <dt>Name
        </dt>

        <dd>
            <label>@(product.Name)</label>
        </dd>

        <dt>Description
        </dt>

        <dd>
            <label>@(product.Description)</label>
        </dd>

        <dt>Brand
        </dt>

        <dd>
            <label>@(product.CatalogBrand.Brand)</label>
        </dd>

        <dt>Type
        </dt>

        <dd>
            <label>@(product.CatalogType.Type)</label>
        </dd>
        <dt>Price
        </dt>

        <dd>
            <label>@(product.Price)</label>
        </dd>

        <dt>Picture name
        </dt>

        <dd>
            <label>@(product.PictureFileName)</label>
        </dd>

        <dt>Stock
        </dt>

        <dd>
            <label>@(product.AvailableStock)</label>
        </dd>

        <dt>Restock
        </dt>

        <dd>
            <label></label>
        </dd>

        <dt>Max stock
        </dt>

        <dd>
            <label id=""label1"">@(product.MaxStockThreshold)</label>
        </dd>

    </dl>
</div>";
            
            Assert.AreEqual(expectedContents, fileContents);
            Assert.True(fileContents.Contains("<label"));
            Assert.True(fileContents.Contains("</label>"));
            Assert.False(fileContents.Contains("asp:Label"));
            Assert.False(fileContents.Contains("Text"));
            Assert.IsTrue(fi.RelativePath.Equals(relativePath));
        }

        [Test]
        public void ConvertEmbeddedCode_Converts_DataBinding()
        {
            string htmlString = @"<div class=""esh-table"">
    <asp:ListView ID=""productList"" ItemPlaceholderID=""itemPlaceHolder"" runat=""server"" ItemType=""eShopLegacyWebForms.Models.CatalogItem"">
        <ItemTemplate>
            <tr>
                <td>
                    <image class=""esh-thumbnail"" src='/Pics/<%#:Item.PictureFileName%>' />
                </td>
                <p>
                    <%#:Item.MaxStockThreshold%>
                </p>
                <td>
                    <asp:HyperLink NavigateUrl='<%# GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) %>' runat=""server"" CssClass=""esh-table-link"">
                        Edit
                    </asp:HyperLink>
                </td>
            </tr>
        </ItemTemplate>
    </asp:ListView>
</div>";
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);
            htmlDoc = ControlConverter.ConvertEmbeddedCode(htmlDoc);
            
            string contents = htmlDoc.DocumentNode.WriteTo();
            string expectedContents = @"<div class=""esh-table"">
    <asp:listview id=""productList"" itemplaceholderid=""itemPlaceHolder"" runat=""server"" itemtype=""eShopLegacyWebForms.Models.CatalogItem"">
        <itemtemplate>
            <tr>
                <td>
                    <image class=""esh-thumbnail"" src='/Pics/@(Item.PictureFileName)'></image>
                </td>
                <p>
                    @(Item.MaxStockThreshold)
                </p>
                <td>
                    <asp:hyperlink navigateurl='@(GetRouteUrl(""EditProductRoute"", new {id =Item.Id}) )' runat=""server"" cssclass=""esh-table-link"">
                        Edit
                    </asp:hyperlink>
                </td>
            </tr>
        </itemtemplate>
    </asp:listview>
</div>";
            Assert.AreEqual(expectedContents, contents);
        }

        [Test]
        public void ConvertEmbeddedCode_Converts_SingExpr()
        {
            string htmlString = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <span class=""esh-pager-item"">Showing <%: Model.ItemsPerPage%> of <%: Model.TotalItems%> products - Page <%: (Model.ActualPage + 1)%> - <%: Model.TotalPages%>
                </span>
            </nav>
        </article>
    </div>
</div>";
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);
            htmlDoc = ControlConverter.ConvertEmbeddedCode(htmlDoc);
            
            string contents = htmlDoc.DocumentNode.WriteTo();
            string expectedContents = @"<div class=""esh-pager"">
    <div class=""container"">
        <article class=""esh-pager-wrapper row"">
            <nav>
                <span class=""esh-pager-item"">Showing @Model.ItemsPerPage of @Model.TotalItems products - Page @(Model.ActualPage + 1) - @Model.TotalPages
                </span>
            </nav>
        </article>
    </div>
</div>";
            Assert.AreEqual(expectedContents, contents);
        }
    }
}