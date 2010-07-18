<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<JavaGeneration.Chirper.Models.ErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Sorry, an error occurred while processing your request.
    </h2>
    
    <strong><%=Html.Encode(Model.Message ?? "Unknown error") %>.</strong>
    <%=Html.Encode(Model.Details ?? string.Empty) %>.
</asp:Content>
