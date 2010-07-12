<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<JavaGeneration.Chirper.Models.User>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Profile
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Profile</h2>

    <%= Html.ValidationSummary("Profile update was unsuccessful. Please correct the errors and try again.") %>
    <% using (Html.BeginForm())
       { %>
    <div>
        <fieldset>
        <%  %>
            <legend><%=Html.Encode(Model.Name) %> Profile</legend>
            <% Html.RenderPartial("UserProfileControl", Model); %>
            <p>
                <%=Html.ActionLink("Change password", "ChangePassword") %>
            </p>
            <p>
                <input type="submit" value="Update" />
            </p>
        </fieldset>
    </div>
    <% } %>
</asp:Content>

