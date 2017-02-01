/*
' Copyright (c) 2017 Hotcakes Commerce, LLC
'  All rights reserved.
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy 
' of this software and associated documentation files (the "Software"), to deal 
' in the Software without restriction, including without limitation the rights 
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
' copies of the Software, and to permit persons to whom the Software is 
' furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all 
' copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
' SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Hotcakes.Commerce;
using Hotcakes.Commerce.Extensions;
using Hotcakes.Commerce.Orders;
using Hotcakes.Commerce.Catalog;

namespace Hotcakes.Modules.AdjustReservedInventoryModule
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Has not been tested against product bundles
    /// </remarks>
    public partial class View : AdjustReservedInventoryModuleBase
    {
        #region Private Members

        private HotcakesApplication _hccApp = null;

        private HotcakesApplication HccApp
        {
            get
            {
                // NOTE: This is using an old method of initiating the HccApp object, 
                // and should be updated if used against Hotcakes 2.0 or newer
                return _hccApp ?? (_hccApp = HccAppHelper.InitHccApp());
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack) BindData();
            }
            catch (Exception exc) 
            {
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ResetInventoryReserve(object sender, EventArgs e)
        {
            GridViewRow grdrow = (GridViewRow)((LinkButton)sender).NamingContainer;
            var sku = grdrow.Cells[0].Text;
            var variantId = grdrow.Cells[2].Text;

            var product = HccApp.CatalogServices.Products.FindBySku(sku);

            var orders = HccApp.OrderServices.Orders.FindAll().Where(o => o.StatusCode == OrderStatusCode.Cancelled);

            var lineItems =
                HccApp.OrderServices.Orders.FindLineItemsForOrders(orders.ToList()).Where(li => li.ProductId == product.Bvin);

            var returnQty = lineItems.Sum(li => li.QuantityReserved);

            if (lineItems.Any())
            {
                // this will need to be done differently in Hotcakes version 2.0 and newer
                LineItemRepository repo = null;
                repo = Factory.CreateRepo<LineItemRepository>(HccApp.CurrentRequestContext);

                foreach (var lineItem in lineItems)
                {
                    lineItem.QuantityReserved = 0;

                    repo.Update(lineItem);
                }
            }

            List<ProductInventory> inventories = null;

            if (string.IsNullOrEmpty(variantId))
            {
                inventories = HccApp.CatalogServices.ProductInventories.FindByProductId(product.Bvin);
            }
            else
            {
                inventories = new List<ProductInventory>
                {
                    HccApp.CatalogServices.ProductInventories.FindByProductIdAndVariantId(product.Bvin, variantId)
                };
            }

            if (inventories != null && inventories.Count > 0)
            {
                foreach (var inventory in inventories)
                {
                    inventory.QuantityReserved = inventory.QuantityReserved - returnQty;
                    HccApp.CatalogServices.ProductInventories.Update(inventory);
                }
            }

            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL());
        }

        #endregion

        #region Helper Methods

        private void BindData()
        {
            LocalizeModule();

            LoadGrid();
        }

        private void LocalizeModule()
        {
            Localization.LocalizeGridView(ref grdProducts, LocalResourceFile);
        }

        private void ShowProducts(bool hasProducts = true)
        {
            pnlNoProducts.Visible = !hasProducts;
            pnlProductList.Visible = hasProducts;
        }

        private void LoadGrid()
        {
            //
            // this is a broad search, and it should instead include some 
            // additional criteria to lessen the number of orders returned, 
            // such as a date range
            //
            var orders = HccApp.OrderServices.Orders.FindAll().Where(o => o.StatusCode == OrderStatusCode.Cancelled);

            if (orders == null || !orders.Any())
            {
                ShowProducts(false);
                return;
            }

            var lineItems = HccApp.OrderServices.Orders.FindLineItemsForOrders(orders.ToList()).Where(li => li.QuantityReserved > 0);
            var filteredLineItems = new List<LineItem>();

            // get a reference to only the products that match the line items we're looking for
            var products = HccApp.CatalogServices.Products.FindAllPaged(1, int.MaxValue).Where(product => lineItems.Any(li => li.ProductId == product.Bvin)).ToList();

            foreach (var lineItem in lineItems)
            {
                // determine if the product has inventory mode set to reserve product
                var productInList = products.Any(p => p.Bvin == lineItem.ProductId && p.InventoryMode != ProductInventoryMode.AlwayInStock && p.InventoryMode != ProductInventoryMode.NotSet);

                if (productInList)
                {
                    filteredLineItems.Add(lineItem);
                }
            }

            if (filteredLineItems.Count > 0)
            {
                ShowProducts();

                grdProducts.DataSource = filteredLineItems;
                grdProducts.DataBind();
            }
            else
            {
                ShowProducts(false);
            }
        }

        protected string GetInventoryText(object oBvin, object oVariant)
        {
            if (oBvin == null) return string.Empty;

            var bvin = oBvin.ToString();
            var variantId = string.Empty;
            if (oVariant != null)
            {
                variantId = oVariant.ToString();
            }

            List<ProductInventory> inventories = null;

            if (string.IsNullOrEmpty(variantId))
            {
                inventories = HccApp.CatalogServices.ProductInventories.FindByProductId(bvin);
            }
            else
            {
                inventories = new List<ProductInventory>
                {
                    HccApp.CatalogServices.ProductInventories.FindByProductIdAndVariantId(bvin, variantId)
                };
            }

            if (inventories == null || inventories.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var inventory in inventories)
            {
                if (sb.Length > 0)
                {
                    sb.AppendFormat("<br />On Hand: {0}; Reserved: {1}; Available: {2};", 
                        inventory.QuantityOnHand,
                        inventory.QuantityReserved,
                        inventory.QuantityAvailableForSale);
                }
                else
                {
                    sb.AppendFormat("On Hand: {0}; Reserved: {1}; Available: {2};", 
                        inventory.QuantityOnHand,
                        inventory.QuantityReserved,
                        inventory.QuantityAvailableForSale);
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}