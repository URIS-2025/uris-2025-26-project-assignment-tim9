# Registar portova i ruta (lokalni dev)

Ovaj fajl postoji zato sto smo primetili da servisi medjusobno pogadjaju portove
u appsettings.json (`Services` / `ServiceUrls` sekcije) i da se ti pogodjeni
portovi ne poklapaju sa stvarnim portovima iz `launchSettings.json` kolega
(npr. Sprint i Timelog referenciraju ProjectService na portu 7001, a Project
stvarno radi na 7065; WorkPackageService je referenciran kao "7002", a stvarno
radi na 7041). Api Gateway (ovaj folder: `ApiGateway/`) rutira prema stvarnim
portovima ispod - ako promenis svoj port u `launchSettings.json`, azuriraj i
`ApiGateway/ApiGateway/appsettings.json` i ovaj fajl.

| Servis | HTTP port (dev) | HTTPS port (dev) | Prefiks ruta (stvaran, iz koda) | Status |
|---|---|---|---|---|
| ApiGateway | 5000 | 7000 | / (prosledjuje dalje) | Implementirano |
| AuthService | 5140 (predlog) | 7140 (predlog) | /auth/** (po dokumentaciji) | Nije zapoceto |
| UserService | 5150 (predlog) | 7150 (predlog) | /users/** (po dokumentaciji) | Nije zapoceto |
| NotificationService | 5100 | 7100 | /notifications/** | Implementirano |
| IntegrationService | 5110 | 7110 | /integrations/** | Implementirano |
| ProjectService | 5230 | 7065 | /api/project/**, /api/milestone/**, /api/projectmember/**, /api/requirements/** | U izradi (feature/project-service) |
| SprintService | 5130 | 7221 | /api/sprint/** | U izradi (feature/sprint-service) |
| TimelogService | 5273 | 7113 | /api/timelog/** | U izradi (feature/timelog-service) |
| WorkPackageService | 5037 | 7041 | /api/workpackage/**, /api/task/**, /api/comment/**, /api/dependency/**, /api/backlog/** | U izradi (feature/workpackage-service) |
| AttachmentService | 5278 | 7075 | /api/attachment/** (predlog - kontroleri jos nisu pisani) | U izradi (feature/attachment-service) |
| PaymentService | 5160 (predlog) | 7160 (predlog) | /payments/**, /invoices/** (po dokumentaciji) | Nije zapoceto |

## Vazna nekonzistentnost izmedju servisa

Timovi su se, bez dogovora, podelili na dve konvencije za rute:

- **ProjectService, SprintService, TimelogService, WorkPackageService** koriste
  default ASP.NET Core scaffolding rutu `[Route("api/[controller]")]`
  (npr. `api/project`, `api/sprint`, `api/task`...).
- **NotificationService i IntegrationService** (ovaj deo) koriste ravne rute
  bez `api/` prefiksa i u jednini/mnozini kako je opisano u originalnoj
  API dokumentaciji (npr. `/notifications`, `/integrations`), zato sto
  WorkPackageService.ServiceCalls.Notification.NotificationService vec
  poziva `POST /notifications` (bez `api/` prefiksa) - taj ugovor je vec
  napisan i testiran, pa ga ne diramo.

Gateway trenutno rutira prema stvarnom stanju svakog servisa (gore u tabeli),
ali bi bilo dobro da se tim dogovori oko jedinstvene konvencije pre finalne
integracije - u suprotnom, svaki novi servis (Auth, User, Payment) treba
najaviti svoju stvarnu rutu ovde cim krene da se pise.

## MySQL (lokalno)

Svi servisi ocekuju MySQL na `localhost:3306` sa `root/root` (ili `root/CHANGE_ME`
u WorkPackageService - proveriti sa autorom). Svaki servis ima svoju bazu
(npr. `NotificationServiceDB`, `IntegrationServiceDB`, `ProjectDB`, `SprintDB`,
`TimelogDB`).
