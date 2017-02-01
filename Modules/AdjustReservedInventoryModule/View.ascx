<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Hotcakes.Modules.AdjustReservedInventoryModule.View" %>
<h1><%=GetLocalizedString("Header") %></h1>
<p><%=GetLocalizedString("Info") %></p>
<asp:Panel id="pnlProductList" runat="server">
    <div class="dnnFormMessage dnnFormValidationSummary"><%=GetLocalizedString("ProductsFound") %></div>
    <div class="dnnClear">
        <asp:GridView id="grdProducts" AutoGenerateColumns="false"
                CssClass="dnnGrid" DataKeyNames="Id" Width="100%" runat="server">
            <Columns>
                <asp:BoundField DataField="ProductSku" HeaderText="Sku" />
                <asp:BoundField DataField="ProductName" HeaderText="ProductName" />
                <asp:BoundField DataField="VariantId" HeaderText="VariantId" />
                <asp:TemplateField HeaderText="Inventory">
                    <ItemTemplate>
                        <%#GetInventoryText(DataBinder.Eval(Container.DataItem, "ProductId"), DataBinder.Eval(Container.DataItem, "VariantId")) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:LinkButton OnClick="ResetInventoryReserve" Text="Restore Cancelled Inventory" runat="server"/>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <HeaderStyle CssClass="dnnGridHeader" />
            <RowStyle CssClass="dnnGridItem" />
            <AlternatingRowStyle CssClass="dnnGridAltItem" />
            <FooterStyle CssClass="dnnGridItem" />
        </asp:GridView>
    </div>
</asp:Panel>
<asp:Panel id="pnlNoProducts" runat="server">
    <div class="dnnFormMessage dnnFormInfo"><%=GetLocalizedString("NoProducts") %></div>
</asp:Panel>