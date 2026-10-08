# ChessAdmin
โครงสร้าง Chess
D:\Project\ChessAdmin\wwwroot\wwwroot
├── assets
├── css
│   ├── adminlte.css
│   ├── ChessBoard.css
│   ├── NotificationToast.css
│   └── style.css
├── js
│   ├── adminlte.js
│   ├── bootstrap.bundle.min.js
│   └── main.js
└── lib
    └── bootstrap
        └── dist
            ├── css
            │   ├── bootstrap.css
            │   ├── bootstrap-grid.css
            │   ├── bootstrap-reboot.css
            │   └── bootstrap-utilities.css
            └── js
                └── bootstrap.js
Components
├── Layout
│   └── MainLayout.razor
└── Models
    ├── AiPerformancDtos.cs
    ├── DashboardOverviewDto.cs
    ├── GameDtos.cs
    ├── LoginModel.cs
    ├── MovesDtos.cs
    ├── NotificationDto.cs
    └── UserDtos.cs
Pages
├── Base
│   ├── Navbar.razor
│   ├── Navbar.razor.cs
│   └── Sidebar.razor
├── Chart
│   ├── ChartGame.razor
│   └── ChartUser.razor
├── Game
│   ├── Details.razor
│   └── List.razor
├── Notifications
│   ├── alerts.cshtml
│   ├── badge.cshtml
│   └── modals.cshtml
├── 404.razor
├── AddUserModal.razor
├── Alperformance.razor
├── Component.razor
├── Error.razor
├── Games.razor
├── Home.razor
├── Index.razor
├── Login.razor
├── NotFound.razor
├── Profile.razor
├── Profile.razor.cs
├── Register.razor
├── Users.razor
├── Users.razor.cs
└── Weather.razor

Shared
├── Chess
├── NotificationCenter.razor
└── NotificationToast.razor
├── _Imports.razor
├── App.razor
├── RedirectToLogin.razor
└── Routes.razor


D:\Project\ChessAdmin\Services\