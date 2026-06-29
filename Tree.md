# File Tree: .ei

**Generated:** 6/29/2026, 12:04:08 PM
**Root Path:** `e:\Codes\Facul\.ei`

```
├── Pontuei.Api
│   ├── Commons
│   │   ├── ErrorMessages.cs
│   │   └── MapsterConfig.cs
│   ├── Controllers
│   │   ├── AuthController.cs
│   │   ├── LoyaltyProgramController.cs
│   │   ├── NotificationController.cs
│   │   ├── PontueiControllerBase.cs
│   │   ├── TransactionController.cs
│   │   └── UserController.cs
│   ├── Data
│   │   ├── PontueiDbContext.cs
│   │   └── UnitOfWork.cs
│   ├── Dtos
│   │   └── AddMediaRequestDto.cs
│   ├── Interfaces
│   │   ├── Jobs
│   │   │   └── IOverdueTransactionJob.cs
│   │   ├── Repositories
│   │   │   ├── IConfigurationRepository.cs
│   │   │   ├── ILoyaltyProgramRepository.cs
│   │   │   ├── IMetadataRepository.cs
│   │   │   ├── INotificationRepository.cs
│   │   │   ├── ITransactionMediaRepository.cs
│   │   │   ├── ITransactionRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── IUserLoyaltyProgramRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IUserSessionRepository.cs
│   │   │   └── IVerificationCodeRepository.cs
│   │   └── Services
│   │       ├── IAuthService.cs
│   │       ├── IConfigurationService.cs
│   │       ├── IEmailService.cs
│   │       ├── ILoyaltyProgramService.cs
│   │       ├── IMetadataService.cs
│   │       ├── INotificationService.cs
│   │       ├── IPushNotificationService.cs
│   │       ├── IStorageService.cs
│   │       ├── ITokenService.cs
│   │       ├── ITransactionService.cs
│   │       ├── IUserLoyaltyProgramService.cs
│   │       └── IUserService.cs
│   ├── Jobs
│   │   ├── OverdueTransactionHostedService.cs
│   │   └── OverdueTransactionJob.cs
│   ├── Models
│   │   ├── Settings
│   │   │   ├── EmailSettings.cs
│   │   │   └── FcmSettubgs.cs
│   │   ├── BaseEntity.cs
│   │   ├── Configuration.cs
│   │   ├── DbVersion.cs
│   │   ├── LoyaltyProgram.cs
│   │   ├── Notification.cs
│   │   ├── Transaction.cs
│   │   ├── TransactionMedia.cs
│   │   ├── User.cs
│   │   ├── UserLoyaltyProgram.cs
│   │   ├── UserSession.cs
│   │   └── VerificationCode.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── Repositories
│   │   ├── BaseRepository.cs
│   │   ├── ConfigurationRepository.cs
│   │   ├── LoyaltyProgramRepository.cs
│   │   ├── MetadataRepository.cs
│   │   ├── NotificationRepository.cs
│   │   ├── TransactionMediaRepository.cs
│   │   ├── TransactionRepository.cs
│   │   ├── UserLoyaltyProgramRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── UserSessionRepository.cs
│   │   └── VerificationCodeRepository.cs
│   ├── Seeders
│   │   ├── AdminUserSeeder.cs
│   │   └── LoyaltyProgramLogoSeeder.cs
│   ├── Services
│   │   ├── AuthService.cs
│   │   ├── ConfigurationService.cs
│   │   ├── EmailService.cs
│   │   ├── FcmPushNotificationService.cs
│   │   ├── LoyaltyProgramService.cs
│   │   ├── MetadataService.cs
│   │   ├── MinioStorageService.cs
│   │   ├── NotificationService.cs
│   │   ├── TokenService.cs
│   │   ├── TransactionService.cs
│   │   ├── UserLoyaltyProgramService.cs
│   │   └── UserService.cs
│   ├── wwwroot
│   │   ├── css
│   │   │   └── custom.css
│   │   ├── img
│   │   │   ├── favicon.ico
│   │   │   ├── logo.png
│   │   │   └── logo.svg
│   │   └── js
│   │       └── custom.js
│   ├── Dockerfile
│   ├── Pontuei.Api.csproj
│   ├── Program.cs
│   └── appsettings.json
├── Pontuei.App
│   ├── Build
│   │   ├── Android
│   │   │   ├── Dockerfile
│   │   └── iOs
│   ├── Controls
│   │   ├── BottomNavBar.xaml
│   │   └── BottomNavBar.xaml.cs
│   ├── Platforms
│   │   ├── Android
│   │   │   ├── Resources
│   │   │   │   ├── values
│   │   │   │   │   └── colors.xml
│   │   │   │   └── xml
│   │   │   │       └── network_security_config.xml
│   │   │   ├── Services
│   │   │   │   └── FirebaseMessagingService.cs
│   │   │   ├── AndroidManifest.xml
│   │   │   ├── GoogleAuthService.cs
│   │   │   ├── MainActivity.cs
│   │   │   └── MainApplication.cs
│   │   ├── MacCatalyst
│   │   │   ├── AppDelegate.cs
│   │   │   ├── Entitlements.plist
│   │   │   ├── Info.plist
│   │   │   └── Program.cs
│   │   ├── Tizen
│   │   │   ├── Main.cs
│   │   │   └── tizen-manifest.xml
│   │   ├── Windows
│   │   │   ├── App.xaml
│   │   │   ├── App.xaml.cs
│   │   │   ├── Package.appxmanifest
│   │   │   └── app.manifest
│   │   └── iOS
│   │       ├── Resources
│   │       │   └── PrivacyInfo.xcprivacy
│   │       ├── AppDelegate.cs
│   │       ├── Info.plist
│   │       └── Program.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── Resources
│   │   ├── AppIcon
│   │   │   └── appicon.svg
│   │   ├── Fonts
│   │   │   ├── BricolageGrotesque
│   │   │   │   ├── BricolageGrotesque-OFL.txt
│   │   │   │   ├── BricolageGrotesqueBold.otf
│   │   │   │   ├── BricolageGrotesqueLight.otf
│   │   │   │   └── BricolageGrotesqueRegular.otf
│   │   │   └── Poppins
│   │   │       ├── poppins-OFL.txt
│   │   │       ├── poppinsBold.ttf
│   │   │       ├── poppinsRegular.ttf
│   │   │       └── poppinsSemibold.ttf
│   │   ├── Images
│   │   │   ├── bell.svg
│   │   │   ├── bell_selected.svg
│   │   │   ├── calendar.svg
│   │   │   ├── calendar_white.svg
│   │   │   ├── cancel.svg
│   │   │   ├── change.svg
│   │   │   ├── change_white.svg
│   │   │   ├── clock.svg
│   │   │   ├── clock_selected.svg
│   │   │   ├── confirm.svg
│   │   │   ├── email.svg
│   │   │   ├── email_white.svg
│   │   │   ├── eye.svg
│   │   │   ├── eye_black.svg
│   │   │   ├── eye_crossed.svg
│   │   │   ├── eye_crossed_black.svg
│   │   │   ├── folder.svg
│   │   │   ├── folder_white.svg
│   │   │   ├── google_icon.svg
│   │   │   ├── home.svg
│   │   │   ├── key.svg
│   │   │   ├── key_hole.svg
│   │   │   ├── key_hole_white.svg
│   │   │   ├── key_white.svg
│   │   │   ├── lock.svg
│   │   │   ├── logo.svg
│   │   │   ├── pen.svg
│   │   │   ├── pen_white.svg
│   │   │   ├── plus.svg
│   │   │   ├── profile.svg
│   │   │   ├── profile_white.svg
│   │   │   ├── settings.svg
│   │   │   ├── trash.svg
│   │   │   └── trash_white.svg
│   │   ├── Raw
│   │   │   └── AboutAssets.txt
│   │   ├── Splash
│   │   │   └── splash.svg
│   │   └── Styles
│   │       ├── Colors.xaml
│   │       └── Styles.xaml
│   ├── Services
│   │   ├── Api
│   │   │   ├── ApiClient.cs
│   │   │   ├── AuthApiService.cs
│   │   │   ├── LoyaltyProgramsApiService.cs
│   │   │   ├── NotificationApiService.cs
│   │   │   ├── TransactionApiService.cs
│   │   │   └── UserApiService.cs
│   │   ├── AppConstants.cs
│   │   └── AuthService.cs
│   ├── ViewModels
│   ├── Views
│   │   ├── AuthPage.xaml
│   │   ├── AuthPage.xaml.cs
│   │   ├── BasePage.xaml
│   │   ├── BasePage.xaml.cs
│   │   ├── ChangeProgramPage.xaml
│   │   ├── ChangeProgramPage.xaml.cs
│   │   ├── HistoryPage.xaml
│   │   ├── HistoryPage.xaml.cs
│   │   ├── HomePage.xaml
│   │   ├── HomePage.xaml.cs
│   │   ├── NotificationsPage.xaml
│   │   ├── NotificationsPage.xaml.cs
│   │   ├── ProgramSelectionPage.xaml
│   │   ├── ProgramSelectionPage.xaml.cs
│   │   ├── ReorderProgramsPage.xaml
│   │   ├── ReorderProgramsPage.xaml.cs
│   │   ├── SettingsPage.xaml
│   │   ├── SettingsPage.xaml.cs
│   │   ├── SplashPage.xaml
│   │   ├── SplashPage.xaml.cs
│   │   ├── TransactionDetailPage.xaml
│   │   ├── TransactionDetailPage.xaml.cs
│   │   ├── TransactionMediaPage.xaml
│   │   └── TransactionMediaPage.xaml.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── AppConfig.cs
│   ├── AppShell.xaml
│   ├── AppShell.xaml.cs
│   ├── Directory.Packages.props
│   ├── MauiProgram.cs
│   ├── Pontuei.App.csproj
│   ├── appsettings.Template.json
│   └── global.json
├── Pontuei.Shared
│   ├── Commons
│   │   └── ValidationUtils.cs
│   ├── Dtos
│   │   ├── Objects
│   │   │   ├── ApiResult.cs
│   │   │   ├── ConfigurationDto.cs
│   │   │   ├── LoyaltyProgramDto.cs
│   │   │   ├── MetadataDtos.cs
│   │   │   ├── NotificationDto.cs
│   │   │   ├── TransactionDto.cs
│   │   │   ├── TransactionMediaDto.cs
│   │   │   ├── UserDto.cs
│   │   │   ├── UserLoyaltyProgramDto.cs
│   │   │   └── UserSessionData.cs
│   │   ├── Requests
│   │   │   ├── ConfigurationCRUDRequestsDtos.cs
│   │   │   ├── ForgetPasswordRequestDtos.cs
│   │   │   ├── LoginRequestDtos.cs
│   │   │   ├── LoyaltyProgramCRUDRequestsDtos.cs
│   │   │   ├── NotificationsRequestsDtos.cs
│   │   │   ├── RefreshTokenRequestDtos.cs
│   │   │   ├── TransactionCRUDRequestsDtos.cs
│   │   │   ├── TransactionMediaCRUDRequestsDtos.cs
│   │   │   ├── UserCRUDRequestsDtos.cs
│   │   │   ├── UserLoyaltyProgramCRUDRequestsDtos.cs
│   │   │   └── VerifyEmailRequestDtos.cs
│   │   └── Responses
│   │       ├── GetDashboardSummaryResponseDtos.cs
│   │       ├── GetResponsesDtos.cs
│   │       ├── LoginResponseDto.cs
│   │       └── ResetPasswordResponseDto.cs
│   ├── Enums
│   │   ├── ConfigurationType.cs
│   │   ├── EmailVerificationCodeType.cs
│   │   ├── TransactionMediaFileType.cs
│   │   └── TransactionStatus.cs
│   └── Pontuei.Shared.csproj
├── assets
│   ├── Brand
│   │   ├── logo-insise-ponto_green.svg
│   │   ├── logo-insise-ponto_white.svg
│   │   ├── logo.svg
│   │   ├── ponto.svg
│   │   └── screenPrototipes.pdf
│   └── LoyaltyPrograms
│       ├── Atomos.webp
│       ├── Azul.webp
│       ├── Caixa.webp
│       ├── Dotz.webp
│       ├── Esfera.webp
│       ├── InterLoop.webp
│       ├── Itau.webp
│       ├── LatamPass.webp
│       ├── Livelo.webp
│       ├── Smiles.webp
│       ├── Stix.webp
│       └── XPInvestimentos.webp
├── database
│   ├── migrations
│   ├── scripts
│   │   ├── P01_000_base.sql
│   │   ├── P02_000_functions.sql
│   │   ├── P03_001_table_db_version.sql
│   │   ├── P03_002_table_configuration.sql
│   │   ├── P03_003_table_user.sql
│   │   ├── P03_010_table_user_session.sql
│   │   ├── P03_020_table_verification_code.sql
│   │   ├── P03_030_table_loyalty_program.sql
│   │   ├── P03_040_table_user_loyalty_program.sql
│   │   ├── P03_050_table_transaction.sql
│   │   ├── P03_060_table_transaction_media.sql
│   │   ├── P03_070_table_notification.sql
│   │   └── P04_001_general_inserts.sql
│   ├── Dockerfile
│   └── setup.sh
├── .env.example
├── .gitignore
├── README.md
├── Tree.md
└── docker-compose.yml
```

---

_Generated by FileTree Pro Extension_
