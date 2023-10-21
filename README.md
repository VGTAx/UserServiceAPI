# UserServiceAPI

The project is an ASP.NET Core Web API for managing users and their roles.
<h4>The project implements the following:</h4>
<ol>
   <li>API Methods</li>
   <li>API documentation using Swagger.</li>
   <li>Logging API actions using Serilog.</li>
</ol>
<details>
<summary><h4>API methods:</h4></summary>
<ol>
   <li><b>Getting a list of all users:</b>
       <ul>
         <li>HTTP method: GET</li>
         <li>Path: /api/UserService/GetUsers</li>
         <li>Description: The method allows you to get a list of all users, configured to support pagination, sorting and filtering by various attributes.</li>
       </ul>
   </li>
   <li><b>Getting user by Id:</b>
       <ul>
         <li>HTTP method: GET</li>
         <li>Path: /api/UserService/GetUser</li>
         <li>Description: The method allows you to obtain information about the user by his unique identifier (Id) along with a list of his roles.</li>
       </ul>
   </li>
   <li><b>Creating a new user:</b>
       <ul>
         <li>HTTP method: POST</li>
         <li>Path: /api/UserService/CreateUser</li>
         <li>Description: The method allows you to create a new user with the specified values.</li>
       </ul>
   </li>
   <li><b>Updating user information:</b>
       <ul>
         <li>HTTP method: POST</li>
         <li>Path: /api/UserService/EditUser</li>
         <li>Description: The method allows you to update information about the user.</li>
       </ul>
   </li>
   <li><b>Changing user roles:</b>
       <ul>
         <li>HTTP method: POST</li>
         <li>Path: /api/UserService/ChangeUserRoles</li>
         <li>Description: The method allows you to change the list of user roles</li>
       </ul>
   </li>
   <li><b>Changing user roles:</b>
       <ul>
         <li>HTTP method: DELETE</li>
         <li>Path: /api/UserService/DeleteUser</li>
         <li>Description: The method allows you to delete a user by his unique identifier (Id).</li>
       </ul>
   </li>
</ol>
</details>

**Access to documentation:**
<ol>
   <li>Launch the API</li>
   <li>Go to https://localhost:port/swagger/index.html in your browser, where "port" is the port the project is running on.</li>
</ol>

<b>Installation and use instructions:</b>
<ol>
   <li>Clone the repository.</li>
   <li>Go to the project directory.</li>
   <li>Make sure you have the .NET SDK installed. If not, install it from the official .NET site: https://dotnet.microsoft.com/download/dotnet</li>
   <li>Restore the project dependencies using the command: <b><i>dotnet restore</i></b></li>
   <li>Run the API using the command: <b><i>dotnet run</i></b></li>
</ol>
