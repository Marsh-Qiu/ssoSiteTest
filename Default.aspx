<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ssoSiteTest._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle">SSO 測試</h1>
            <p>
                <button type="button" class="btn btn-primary tn-md" runat="server" OnServerClick="OnServerClick">登入測試</button>
            </p>
        </section>

      
    </main>

</asp:Content>
