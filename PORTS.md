# Registar portova i ruta (lokalni dev)

Ovaj fajl postoji zato sto smo primetili da servisi medjusobno pogadjaju portove
u appsettings.json (`Services` / `ServiceUrls` sekcije) i da se ti pogodjeni
portovi ne poklapaju uvek sa stvarnim portovima iz `launchSettings.json` kolega.
Api Gateway (ovaj folder: `ApiGateway/`) rutira prema stvarnim portovima i rutama
ispod, potvrdjenim citanjem koda sa svake `feature/*` grane (2026-08-19) - ako
promenis svoj port ili rutu, azuriraj i `ApiGateway/ApiGateway/appsettings.json`
i ovaj fajl.

| Servis | HTTP port (dev) | HTTPS port (dev) | Prefiks ruta (stvaran, iz koda) | Status |
|---|---|---|---|---|
| ApiGateway | 5000 | 7000 | / (prosledjuje dalje) | Implementirano |
| AuthService | 5232 | 7067 | `/api/auth/**` (login, refresh, logout, revoke/{userId}, sessions/{userId}) | Implementirano (feature/auth-service) |
| UserService | 5231 | 7066 | `/api/user/**` (base je `api/user`, jednina - `[Route("api/[controller]")]` na `UserController`) | Implementirano (feature/user-service) |
| NotificationService | 5100 | 7100 | `/notifications/**` | Implementirano |
| IntegrationService | 5110 | 7110 | `/integrations/**` | Implementirano |
| ProjectService | 5230 | 7065 | `/api/project/**`, `/api/milestone/**`, `/api/projectmember/**`, `/api/requirements/**` | Implementirano (feature/project-service) |
| SprintService | 5130 | 7221 | `/sprints/**` (base `sprints`) + `/projects/{projectId}/sprints` (apsolutni override, `~/`) | Implementirano (feature/sprint-service) |
| TimelogService | 5273 | 7113 | `/api/timelog/**` | Implementirano (feature/timelog-service) |
| WorkPackageService | 5037 | 7041 | `/api/workpackage/**`, `/api/task/**`, `/api/comment/**`, `/api/dependency/**`, `/api/backlog/**` | Implementirano (feature/workpackage-service) |
| AttachmentService | 5278 | 7075 | `/attachments/**` (base `attachments`) + `/tasks/{taskId}/attachments` (apsolutni override, `~/`) | Implementirano (feature/attachment-service) |
| PaymentService | 5240 | 7090 | (kontroleri jos nisu napisani - samo modeli/DTO-i) | U izradi (feature/payment-service) |

Gateway (`ApiGateway/ApiGateway/appsettings.json`) je usaglašen sa gornjom tabelom.
Payment/Invoice rute u gateway-u su i dalje po dokumentaciji (`/payments/**`,
`/invoices/**`) jer PaymentService jos nema kontrolere - proveriti i ispraviti
cim se kontroleri napisu.

## Vazna nekonzistentnost izmedju servisa

Timovi su se, bez dogovora, podelili na konvencije za rute:

- **ProjectService, TimelogService, WorkPackageService** koriste default ASP.NET
  Core scaffolding rutu `[Route("api/[controller]")]` (npr. `api/project`,
  `api/timelog`, `api/task`...).
- **AuthService, UserService** takodje koriste `api/[controller]`, sto daje
  `api/auth`, `api/user` (jednina, ne `api/users`!).
- **NotificationService, IntegrationService** koriste ravne rute bez `api/`
  prefiksa, mnozina (`/notifications`, `/integrations`), zato sto
  WorkPackageService.ServiceCalls.Notification.NotificationService vec poziva
  `POST /notifications` (bez `api/` prefiksa) - taj ugovor je vec napisan i
  testiran, pa ga ne diramo.
- **SprintService, AttachmentService** biraju treci obrazac: osnovna ruta je
  ravna mnozina (`/sprints`, `/attachments`), ALI dodaju i po jednu apsolutnu
  rutu preko `[Http...("~/...")]` da bi pokrili "nested" oblik iz zvanicne
  dokumentacije (`/projects/{projectId}/sprints`, `/tasks/{taskId}/attachments`).
  Gateway mora imati DVE rute za svaki od ova dva servisa (videti appsettings.json:
  `sprint-route` + `sprint-project-route`, `attachment-route` + `attachment-task-route`).

Gateway trenutno rutira prema stvarnom stanju svakog servisa (gore u tabeli).
Bilo bi dobro da se tim dogovori oko jedinstvene konvencije pre finalne
integracije - u suprotnom, svaki novi kontroler treba najaviti svoju stvarnu
rutu ovde cim krene da se pise.

## Poznati problem kod kolega (nije nasa grana, samo evidencija)

`SprintService/appsettings.json` i `TimelogService/appsettings.json` (na
`feature/sprint-service` i `feature/timelog-service`) i dalje referenciraju
pogodjene portove za tudje servise koji se ne poklapaju sa stvarnim stanjem:
- Sprint i Timelog imaju `"ProjectService": "https://localhost:7001/"` - stvarni
  https port ProjectService-a je **7065**.
- Timelog ima `"WorkPackageService": "https://localhost:7002/"` - stvarni https
  port WorkPackageService-a je **7041**.

Treba javiti kolegama koje rade Sprint/Timelog da isprave svoje `Services`
sekcije pre integracije, u suprotnom njihovi pozivi ka Project/WorkPackage
servisu ce padati na `ConnectionRefused`.

## MySQL (lokalno)

Svi servisi ocekuju MySQL na `localhost:3306` sa `root/root` (ili `root/CHANGE_ME`
u WorkPackageService - proveriti sa autorom). Svaki servis ima svoju bazu
(npr. `NotificationServiceDB`, `IntegrationServiceDB`, `ProjectDB`, `SprintDB`,
`TimelogDB`).
