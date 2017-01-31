<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Hotcakes.Modules.AdjustReservedInventoryModule.View" %>
<h1><%=GetLocalizedString("Header") %></h1>
<p><%=GetLocalizedString("Info") %></p>
<asp:Panel id="pnlProductList" runat="server">
    <div class="dnnFormMessage dnnFormValidationSummary"><%=GetLocalizedString("ProductsFound") %></div>
    <div class="dnnClear">
        <asp:GridView id="grdProducts" AutoGenerateColumns="false" 
                CssClass="dnnGrid" DataKeyNames="Bvin" Width="100%" runat="server">
            <Columns>
                <asp:BoundField DataField="Sku" HeaderText="Sku" />
                <asp:BoundField DataField="ProductName" HeaderText="ProductName" />
                <asp:TemplateField HeaderText="Inventory">
                    <ItemTemplate>
                        <%#GetInventoryText(DataBinder.Eval(Container.DataItem, "bvin")) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <!-- 
                            Links to Edit Here 
                            - Reset Reserve to Zero
                            - Return Cancelled Inventory
                        -->
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