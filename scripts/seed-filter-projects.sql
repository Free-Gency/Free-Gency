/*
================================================================================
 FreeGency — Seed 16 diverse projects for filter / home testing
================================================================================
 Safe to re-run: skips when projects with Id prefix 44444444-4444-4444-4444 exist.

 Adds:
   - Missing taxonomy categories / specialties / skills (by name)
   - Extra teams (PixelCraft, CloudOps, SecureShield, DataNest, ShopForge)
   - 16 marketplace projects owned by Omar (client3)
   - Cover images via ProjectFiles (home design assets under /assets/client-home/)
   - ProjectProposals (team + solo) across Open / InProgress / Completed

 Demo client: Omar  aaaaaaaa-aaaa-aaaa-aaaa-000000000003
 Existing teams reused: WebSquad, MobileForge
================================================================================
*/

/* Target DB via sqlcmd -d (FreeGency_DB locally, or deployed catalog name). */

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM [marketplace].[Projects]
    WHERE [Id] = '44444444-4444-4444-4444-000000000001'
)
BEGIN
    SELECT N'already_seeded' AS Result,
           COUNT(*) AS ProjectCount
    FROM [marketplace].[Projects]
    WHERE [Id] LIKE '44444444-4444-4444-4444-%';
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @ClientId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003'; /* Omar */
DECLARE @Dev1 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000011';
DECLARE @Dev2 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000012';
DECLARE @Dev3 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000013';
DECLARE @Dev4 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000014';
DECLARE @Dev5 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000015';

IF NOT EXISTS (SELECT 1 FROM [identity].[ClientProfiles] WHERE [UserId] = @ClientId)
BEGIN
    SELECT N'omar_client_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'system';

/* -------------------------------------------------------------------------- */
/* Ensure categories (TaxonomySeedData fixed IDs)                             */
/* -------------------------------------------------------------------------- */

;WITH NeededCats AS (
    SELECT * FROM (VALUES
        ('11111111-1111-1111-1111-000000000001', N'Web Development', N'تطوير الويب'),
        ('11111111-1111-1111-1111-000000000002', N'Mobile Development', N'تطوير الموبايل'),
        ('11111111-1111-1111-1111-000000000003', N'Custom Software / SaaS', N'برمجيات مخصصة / SaaS'),
        ('11111111-1111-1111-1111-000000000004', N'UI/UX for Software', N'تصميم واجهات البرمجيات'),
        ('11111111-1111-1111-1111-000000000005', N'Backend & APIs', N'الباك اند وواجهات البرمجة'),
        ('11111111-1111-1111-1111-000000000006', N'DevOps & Cloud', N'ديف أوبس والسحابة'),
        ('11111111-1111-1111-1111-000000000007', N'QA & Testing', N'ضمان الجودة والاختبار'),
        ('11111111-1111-1111-1111-000000000008', N'Data & AI', N'البيانات والذكاء الاصطناعي'),
        ('11111111-1111-1111-1111-000000000009', N'Cybersecurity', N'الأمن السيبراني'),
        ('11111111-1111-1111-1111-00000000000a', N'E-commerce Development', N'تطوير التجارة الإلكترونية')
    ) v(Id, NameEn, NameAr)
)
INSERT INTO [catalog].[Categories] ([Id], [Name], [NameEn], [CreatedAt], [CreatedBy], [IsDeleted])
SELECT c.Id, c.NameAr, c.NameEn, @Now, @By, 0
FROM NeededCats c
WHERE NOT EXISTS (
    SELECT 1 FROM [catalog].[Categories] x
    WHERE x.[Id] = c.Id OR x.[NameEn] = c.NameEn
);

/* -------------------------------------------------------------------------- */
/* Ensure specialties                                                         */
/* -------------------------------------------------------------------------- */

;WITH NeededSpecs AS (
    SELECT * FROM (VALUES
        (N'Frontend Development', N'تطوير الواجهات الأمامية'),
        (N'Full Stack Development', N'تطوير Full Stack'),
        (N'Landing Pages & Marketing Sites', N'صفحات هبوط ومواقع تسويقية'),
        (N'API Development', N'تطوير واجهات البرمجة'),
        (N'Backend Development', N'تطوير الباك اند'),
        (N'Cross-Platform Mobile Development', N'تطوير موبايل Cross-Platform'),
        (N'Mobile UI Implementation', N'تنفيذ واجهات الموبايل'),
        (N'Internal Tools / Admin Panels', N'أدوات داخلية / لوحات تحكم'),
        (N'MVP / Prototype Development', N'تطوير MVP / Prototype'),
        (N'Design Systems', N'أنظمة التصميم'),
        (N'Dashboard / SaaS Design', N'تصميم Dashboard / SaaS'),
        (N'CI/CD Pipelines', N'خطوط CI/CD'),
        (N'Cloud Infrastructure', N'بنية سحابية'),
        (N'Docker & Kubernetes', N'Docker و Kubernetes'),
        (N'Automated Testing', N'اختبار آلي'),
        (N'API Testing', N'اختبار APIs'),
        (N'NLP / LLMs / Chatbots', N'NLP / LLMs / Chatbots'),
        (N'Penetration Testing', N'اختبار الاختراق'),
        (N'Application Security', N'أمان التطبيقات'),
        (N'Secure Code Review', N'مراجعة الكود الأمنية'),
        (N'Shopify Development', N'تطوير Shopify'),
        (N'Payment Gateway Integration', N'تكامل بوابات الدفع'),
        (N'WordPress / CMS Development', N'تطوير WordPress / CMS'),
        (N'iOS Development', N'تطوير iOS'),
        (N'Data Analysis & BI', N'تحليل البيانات / BI'),
        (N'Marketplace Platforms', N'منصات Marketplace')
    ) v(NameEn, NameAr)
)
INSERT INTO [catalog].[Specialties] ([Id], [NameEn], [NameAr], [CreatedAt], [CreatedBy], [IsDeleted])
SELECT NEWID(), s.NameEn, s.NameAr, @Now, @By, 0
FROM NeededSpecs s
WHERE NOT EXISTS (
    SELECT 1 FROM [catalog].[Specialties] x WHERE x.[NameEn] = s.NameEn
);

/* -------------------------------------------------------------------------- */
/* Ensure skills                                                              */
/* -------------------------------------------------------------------------- */

;WITH NeededSkills AS (
    SELECT v.Name FROM (VALUES
        (N'React'), (N'TypeScript'), (N'Tailwind CSS'), (N'Next.js'), (N'NestJS'),
        (N'Node.js'), (N'PostgreSQL'), (N'REST'), (N'Flutter'), (N'Dart'),
        (N'Firebase'), (N'ASP.NET Core'), (N'C#'), (N'SQL Server'), (N'Figma'),
        (N'Design Systems'), (N'UI Design'), (N'Docker'), (N'Azure'), (N'GitHub Actions'),
        (N'Kubernetes'), (N'Playwright'), (N'Jest'), (N'API Testing'), (N'Python'),
        (N'OpenAI API'), (N'LangChain'), (N'NLP'), (N'OWASP'), (N'Penetration Testing'),
        (N'Burp Suite'), (N'Shopify'), (N'JavaScript'), (N'Stripe'), (N'Angular'),
        (N'RxJS'), (N'WordPress'), (N'PHP'), (N'CSS'), (N'Swift'),
        (N'SwiftUI'), (N'iOS SDK'), (N'Power BI'), (N'SQL'), (N'Pandas'),
        (N'Magento'), (N'MySQL')
    ) v(Name)
)
INSERT INTO [catalog].[Skills] ([Id], [Name], [CreatedAt], [CreatedBy], [IsDeleted])
SELECT NEWID(), s.Name, @Now, @By, 0
FROM NeededSkills s
WHERE NOT EXISTS (
    SELECT 1 FROM [catalog].[Skills] x WHERE x.[Name] = s.Name
);

/* Resolve taxonomy */
DECLARE @CatWeb UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development');
DECLARE @CatMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development');
DECLARE @CatSaas UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Custom Software / SaaS');
DECLARE @CatUiux UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'UI/UX for Software');
DECLARE @CatBackend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Backend & APIs');
DECLARE @CatDevops UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'DevOps & Cloud');
DECLARE @CatQa UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'QA & Testing');
DECLARE @CatDataAi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Data & AI');
DECLARE @CatSecurity UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Cybersecurity');
DECLARE @CatEcommerce UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'E-commerce Development');

IF @CatWeb IS NULL OR @CatMobile IS NULL OR @CatBackend IS NULL
BEGIN
    SELECT N'taxonomy_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @SpecFrontend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Frontend Development');
DECLARE @SpecFullStack UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Full Stack Development');
DECLARE @SpecLanding UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Landing Pages & Marketing Sites');
DECLARE @SpecApi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'API Development');
DECLARE @SpecBackend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Backend Development');
DECLARE @SpecCrossMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Cross-Platform Mobile Development');
DECLARE @SpecMobileUi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Mobile UI Implementation');
DECLARE @SpecAdmin UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Internal Tools / Admin Panels');
DECLARE @SpecMvp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'MVP / Prototype Development');
DECLARE @SpecDesignSys UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Design Systems');
DECLARE @SpecDashDesign UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Dashboard / SaaS Design');
DECLARE @SpecCicd UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'CI/CD Pipelines');
DECLARE @SpecCloud UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Cloud Infrastructure');
DECLARE @SpecDocker UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Docker & Kubernetes');
DECLARE @SpecAutoTest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Automated Testing');
DECLARE @SpecApiTest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'API Testing');
DECLARE @SpecNlp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'NLP / LLMs / Chatbots');
DECLARE @SpecPentest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Penetration Testing');
DECLARE @SpecAppSec UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Application Security');
DECLARE @SpecCodeReview UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Secure Code Review');
DECLARE @SpecShopify UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Shopify Development');
DECLARE @SpecPayment UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Payment Gateway Integration');
DECLARE @SpecWp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'WordPress / CMS Development');
DECLARE @SpecIos UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'iOS Development');
DECLARE @SpecBi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Data Analysis & BI');
DECLARE @SpecMarketplace UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Marketplace Platforms');

DECLARE @SkillReact UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'React');
DECLARE @SkillTs UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'TypeScript');
DECLARE @SkillTw UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Tailwind CSS');
DECLARE @SkillNext UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Next.js');
DECLARE @SkillNest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'NestJS');
DECLARE @SkillNode UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Node.js');
DECLARE @SkillPg UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'PostgreSQL');
DECLARE @SkillRest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'REST');
DECLARE @SkillFlutter UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Flutter');
DECLARE @SkillDart UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Dart');
DECLARE @SkillFirebase UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Firebase');
DECLARE @SkillAsp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'ASP.NET Core');
DECLARE @SkillCs UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'C#');
DECLARE @SkillSqlServer UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'SQL Server');
DECLARE @SkillFigma UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Figma');
DECLARE @SkillDesignSys UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Design Systems');
DECLARE @SkillUi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'UI Design');
DECLARE @SkillDocker UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Docker');
DECLARE @SkillAzure UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Azure');
DECLARE @SkillGha UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'GitHub Actions');
DECLARE @SkillK8s UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Kubernetes');
DECLARE @SkillPlaywright UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Playwright');
DECLARE @SkillJest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Jest');
DECLARE @SkillApiTesting UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'API Testing');
DECLARE @SkillPython UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Python');
DECLARE @SkillOpenai UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'OpenAI API');
DECLARE @SkillLangchain UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'LangChain');
DECLARE @SkillNlp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'NLP');
DECLARE @SkillOwasp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'OWASP');
DECLARE @SkillPentest UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Penetration Testing');
DECLARE @SkillBurp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Burp Suite');
DECLARE @SkillShopify UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Shopify');
DECLARE @SkillJs UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'JavaScript');
DECLARE @SkillStripe UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Stripe');
DECLARE @SkillAngular UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Angular');
DECLARE @SkillRxjs UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'RxJS');
DECLARE @SkillWp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'WordPress');
DECLARE @SkillPhp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'PHP');
DECLARE @SkillCss UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'CSS');
DECLARE @SkillSwift UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Swift');
DECLARE @SkillSwiftUi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'SwiftUI');
DECLARE @SkillIosSdk UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'iOS SDK');
DECLARE @SkillPowerBi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Power BI');
DECLARE @SkillSql UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'SQL');
DECLARE @SkillPandas UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Pandas');
DECLARE @SkillMagento UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Magento');
DECLARE @SkillMysql UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'MySQL');

/* -------------------------------------------------------------------------- */
/* Teams (reuse existing + create new)                                        */
/* -------------------------------------------------------------------------- */

DECLARE @TeamWeb UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000001';
DECLARE @TeamMobile UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000002';
DECLARE @TeamDesign UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000003';
DECLARE @TeamCloud UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000004';
DECLARE @TeamSecure UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000005';
DECLARE @TeamData UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000006';
DECLARE @TeamShop UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000007';

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamWeb)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamWeb, @Dev1, N'WebSquad', NULL, N'WEBSQUAD', N'Web & SaaS delivery squad.', 4.80, 4, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamWeb, @Dev1, N'TeamLeader', N'Tech Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamWeb, @Dev2, N'TeamMember', N'Frontend', CAST(@Now AS date), @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamMobile)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamMobile, @Dev3, N'MobileForge', NULL, N'MOBFORGE', N'Cross-platform mobile specialists.', 4.50, 2, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamMobile, @Dev3, N'TeamLeader', N'Mobile Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamMobile, @Dev5, N'TeamMember', N'QA/DevOps', CAST(@Now AS date), @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamDesign)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamDesign, @Dev2, N'PixelCraft', N'/assets/client-home/brutalist-portfolio.jpg', N'PIXEL01', N'UI/UX and design systems studio.', 4.70, 3, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamDesign, @Dev2, N'TeamLeader', N'Design Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamDesign, @Dev5, N'TeamMember', N'Product Designer', CAST(@Now AS date), @Now, @By, 0);

    INSERT INTO [teams].[TeamCategories]
        ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamDesign, @CatUiux, 1, @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamCloud)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamCloud, @Dev5, N'CloudOps', N'/assets/client-home/cyber-sentinel-lp.jpg', N'CLOUDOPS', N'CI/CD, Azure, and Kubernetes delivery.', 4.60, 2, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamCloud, @Dev5, N'TeamLeader', N'DevOps Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamCloud, @Dev4, N'TeamMember', N'Cloud Engineer', CAST(@Now AS date), @Now, @By, 0);

    INSERT INTO [teams].[TeamCategories]
        ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamCloud, @CatDevops, 1, @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamSecure)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamSecure, @Dev4, N'SecureShield', N'/assets/client-home/stories-app-ui-kit.jpg', N'SECURE01', N'AppSec and penetration testing.', 4.90, 5, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamSecure, @Dev4, N'TeamLeader', N'Security Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamSecure, @Dev1, N'TeamMember', N'Secure Code Reviewer', CAST(@Now AS date), @Now, @By, 0);

    INSERT INTO [teams].[TeamCategories]
        ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamSecure, @CatSecurity, 1, @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamData)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamData, @Dev1, N'DataNest', N'/assets/client-home/e-learning-dashboard.jpg', N'DATANEST', N'Data, BI, and LLM solutions.', 4.75, 3, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamData, @Dev1, N'TeamLeader', N'Data Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamData, @Dev2, N'TeamMember', N'ML Engineer', CAST(@Now AS date), @Now, @By, 0);

    INSERT INTO [teams].[TeamCategories]
        ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamData, @CatDataAi, 1, @Now, @By, 0);
END;

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamShop)
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@TeamShop, @Dev3, N'ShopForge', N'/assets/client-home/nomad-finance-app.jpg', N'SHOPFORGE', N'Shopify and e-commerce specialists.', 4.40, 2, @Now, @By, 0);

    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @TeamShop, @Dev3, N'TeamLeader', N'Commerce Lead', CAST(@Now AS date), @Now, @By, 0),
        (NEWID(), @TeamShop, @Dev2, N'TeamMember', N'Storefront Dev', CAST(@Now AS date), @Now, @By, 0);

    INSERT INTO [teams].[TeamCategories]
        ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamShop, @CatEcommerce, 1, @Now, @By, 0);
END;

/* -------------------------------------------------------------------------- */
/* Projects                                                                   */
/* -------------------------------------------------------------------------- */

DECLARE @P1  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000001';
DECLARE @P2  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000002';
DECLARE @P3  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000003';
DECLARE @P4  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000004';
DECLARE @P5  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000005';
DECLARE @P6  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000006';
DECLARE @P7  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000007';
DECLARE @P8  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000008';
DECLARE @P9  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000009';
DECLARE @P10 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000010';
DECLARE @P11 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000011';
DECLARE @P12 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000012';
DECLARE @P13 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000013';
DECLARE @P14 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000014';
DECLARE @P15 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000015';
DECLARE @P16 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000016';

INSERT INTO [marketplace].[Projects]
(
    [Id], [CreatedAt], [CreatedBy], [IsDeleted], [Title], [Description], [ClientId], [CategoryId],
    [IsFixedPrice], [BudgetMin], [BudgetMax], [Currency], [Deadline], [EstimatedDurationDays],
    [Status], [AssignedTeamId], [AssignedUserId], [CompletedAt]
)
VALUES
(@P1,  DATEADD(MINUTE, -1,  @Now), @By, 0, N'React Dashboard for Analytics', N'Build a modern analytics dashboard with charts, filters, and role-based access. Prefer React + TypeScript.', @ClientId, @CatWeb, 1, 800, 1500, N'USD', DATEADD(DAY, 45, CAST(@Now AS date)), 30, N'Open', NULL, NULL, NULL),
(@P2,  DATEADD(MINUTE, -2,  @Now), @By, 0, N'Landing Page for Startup Launch', N'Marketing landing page with hero, pricing, and contact form. Fast Next.js site with SEO basics.', @ClientId, @CatWeb, 1, 300, 600, N'USD', DATEADD(DAY, 20, CAST(@Now AS date)), 10, N'Open', NULL, NULL, NULL),
(@P3,  DATEADD(MINUTE, -3,  @Now), @By, 0, N'NestJS REST API for Inventory', N'Design and implement a NestJS inventory API with auth, pagination, and PostgreSQL.', @ClientId, @CatBackend, 0, 2000, 4500, N'USD', DATEADD(DAY, 60, CAST(@Now AS date)), 45, N'Open', NULL, NULL, NULL),
(@P4,  DATEADD(MINUTE, -4,  @Now), @By, 0, N'Flutter Cross-Platform Delivery App', N'Mobile delivery tracking app for iOS and Android using Flutter with real-time updates.', @ClientId, @CatMobile, 1, 5000, 9000, N'USD', DATEADD(DAY, 120, CAST(@Now AS date)), 90, N'Open', NULL, NULL, NULL),
(@P5,  DATEADD(MINUTE, -5,  @Now), @By, 0, N'SaaS Admin Panel MVP', N'MVP admin panel for a B2B SaaS: users, billing flags, and basic reports.', @ClientId, @CatSaas, 1, 3500, 7000, N'USD', DATEADD(DAY, 75, CAST(@Now AS date)), 60, N'Open', NULL, NULL, NULL),
(@P6,  DATEADD(MINUTE, -6,  @Now), @By, 0, N'Figma Design System for Fintech', N'Create a complete UI kit and design system in Figma for a fintech dashboard product.', @ClientId, @CatUiux, 1, 1200, 2500, N'EUR', DATEADD(DAY, 40, CAST(@Now AS date)), 25, N'Open', NULL, NULL, NULL),
(@P7,  DATEADD(MINUTE, -7,  @Now), @By, 0, N'CI/CD Pipeline on Azure', N'Set up GitHub Actions and Azure deployment with Docker, staging, and production environments.', @ClientId, @CatDevops, 0, 1500, 3000, N'USD', DATEADD(DAY, 35, CAST(@Now AS date)), 20, N'Open', NULL, NULL, NULL),
(@P8,  DATEADD(MINUTE, -8,  @Now), @By, 0, N'Automated E2E Suite with Playwright', N'Build Playwright end-to-end tests covering auth, checkout, and regression for a web app.', @ClientId, @CatQa, 1, 900, 1800, N'USD', DATEADD(DAY, 30, CAST(@Now AS date)), 21, N'Open', NULL, NULL, NULL),
(@P9,  DATEADD(MINUTE, -9,  @Now), @By, 0, N'LLM Customer Support Chatbot', N'Integrate an NLP chatbot using OpenAI / LangChain for Arabic and English support tickets.', @ClientId, @CatDataAi, 1, 4000, 8000, N'USD', DATEADD(DAY, 70, CAST(@Now AS date)), 50, N'Open', NULL, NULL, NULL),
(@P10, DATEADD(MINUTE, -10, @Now), @By, 0, N'OWASP Security Review for Web App', N'Penetration testing and secure code review focused on auth, XSS, and API threats.', @ClientId, @CatSecurity, 1, 2500, 5000, N'USD', DATEADD(DAY, 25, CAST(@Now AS date)), 15, N'Open', NULL, NULL, NULL),
(@P11, DATEADD(MINUTE, -11, @Now), @By, 0, N'Shopify Store Customization', N'Customize a Shopify theme, add payment options, and improve product listing UX.', @ClientId, @CatEcommerce, 1, 15000, 35000, N'EGP', DATEADD(DAY, 40, CAST(@Now AS date)), 30, N'Open', NULL, NULL, NULL),
(@P12, DATEADD(MINUTE, -12, @Now), @By, 0, N'Hourly Angular Portal Enhancements', N'Ongoing Angular enhancements for an internal portal - flexible hourly engagement.', @ClientId, @CatWeb, 0, 500, 2000, N'EGP', NULL, NULL, N'Open', NULL, NULL, NULL),
(@P13, DATEADD(MINUTE, -13, @Now), @By, 0, N'Draft: WordPress Blog Redesign', N'Draft project - redesign a WordPress blog with new theme and performance tweaks.', @ClientId, @CatWeb, 1, 400, 900, N'USD', NULL, 14, N'Draft', NULL, NULL, NULL),
(@P14, DATEADD(MINUTE, -14, @Now), @By, 0, N'In Progress: iOS Fitness Tracker', N'Native iOS fitness tracking app currently in progress with SwiftUI.', @ClientId, @CatMobile, 1, 6000, 10000, N'USD', DATEADD(DAY, 90, CAST(@Now AS date)), 80, N'InProgress', @TeamMobile, NULL, NULL),
(@P15, DATEADD(MINUTE, -15, @Now), @By, 0, N'Completed: Power BI Sales Reports', N'Completed BI dashboards for sales KPIs using Power BI and SQL.', @ClientId, @CatDataAi, 1, 1000, 2000, N'USD', DATEADD(DAY, -5, CAST(@Now AS date)), 18, N'Completed', @TeamData, NULL, DATEADD(DAY, -2, @Now)),
(@P16, DATEADD(MINUTE, -16, @Now), @By, 0, N'Cancelled: Magento Marketplace', N'Cancelled custom Magento marketplace - kept for status filter testing.', @ClientId, @CatEcommerce, 0, 8000, 15000, N'USD', NULL, 120, N'Cancelled', NULL, NULL, NULL);

/* Specialties */
INSERT INTO [marketplace].[ProjectSpecialties] ([Id], [CreatedAt], [CreatedBy], [IsDeleted], [ProjectId], [SpecialtyId])
SELECT NEWID(), @Now, @By, 0, v.ProjectId, v.SpecialtyId
FROM (VALUES
    (@P1, @SpecFrontend), (@P1, @SpecFullStack),
    (@P2, @SpecLanding),
    (@P3, @SpecApi), (@P3, @SpecBackend),
    (@P4, @SpecCrossMobile), (@P4, @SpecMobileUi),
    (@P5, @SpecAdmin), (@P5, @SpecMvp),
    (@P6, @SpecDesignSys), (@P6, @SpecDashDesign),
    (@P7, @SpecCicd), (@P7, @SpecCloud), (@P7, @SpecDocker),
    (@P8, @SpecAutoTest), (@P8, @SpecApiTest),
    (@P9, @SpecNlp),
    (@P10, @SpecPentest), (@P10, @SpecAppSec), (@P10, @SpecCodeReview),
    (@P11, @SpecShopify), (@P11, @SpecPayment),
    (@P12, @SpecFrontend),
    (@P13, @SpecWp),
    (@P14, @SpecIos),
    (@P15, @SpecBi),
    (@P16, @SpecMarketplace)
) v(ProjectId, SpecialtyId)
WHERE v.SpecialtyId IS NOT NULL;

/* Skills */
INSERT INTO [marketplace].[ProjectSkills] ([Id], [CreatedAt], [CreatedBy], [IsDeleted], [ProjectId], [SkillId])
SELECT NEWID(), @Now, @By, 0, v.ProjectId, v.SkillId
FROM (VALUES
    (@P1, @SkillReact), (@P1, @SkillTs), (@P1, @SkillTw),
    (@P2, @SkillNext), (@P2, @SkillReact), (@P2, @SkillTw),
    (@P3, @SkillNest), (@P3, @SkillNode), (@P3, @SkillPg), (@P3, @SkillRest),
    (@P4, @SkillFlutter), (@P4, @SkillDart), (@P4, @SkillFirebase), (@P4, @SkillRest),
    (@P5, @SkillAsp), (@P5, @SkillCs), (@P5, @SkillSqlServer), (@P5, @SkillReact),
    (@P6, @SkillFigma), (@P6, @SkillDesignSys), (@P6, @SkillUi),
    (@P7, @SkillDocker), (@P7, @SkillAzure), (@P7, @SkillGha), (@P7, @SkillK8s),
    (@P8, @SkillPlaywright), (@P8, @SkillJest), (@P8, @SkillApiTesting),
    (@P9, @SkillPython), (@P9, @SkillOpenai), (@P9, @SkillLangchain), (@P9, @SkillNlp),
    (@P10, @SkillOwasp), (@P10, @SkillPentest), (@P10, @SkillBurp),
    (@P11, @SkillShopify), (@P11, @SkillJs), (@P11, @SkillStripe),
    (@P12, @SkillAngular), (@P12, @SkillTs), (@P12, @SkillRxjs),
    (@P13, @SkillWp), (@P13, @SkillPhp), (@P13, @SkillCss),
    (@P14, @SkillSwift), (@P14, @SkillSwiftUi), (@P14, @SkillIosSdk),
    (@P15, @SkillPowerBi), (@P15, @SkillSql), (@P15, @SkillPandas),
    (@P16, @SkillMagento), (@P16, @SkillPhp), (@P16, @SkillMysql)
) v(ProjectId, SkillId)
WHERE v.SkillId IS NOT NULL;

/* Cover images from home design assets */
INSERT INTO [marketplace].[ProjectFiles]
    ([Id], [ProjectId], [MilestoneId], [UploadedByUserId], [FileName], [FileUrl], [FileKind], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @P1,  NULL, @ClientId, N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P2,  NULL, @ClientId, N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P3,  NULL, @ClientId, N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P4,  NULL, @ClientId, N'cover-social-map-interface.jpg', N'/assets/client-home/social-map-interface.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P5,  NULL, @ClientId, N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P6,  NULL, @ClientId, N'cover-brutalist-portfolio.jpg', N'/assets/client-home/brutalist-portfolio.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P7,  NULL, @ClientId, N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P8,  NULL, @ClientId, N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P9,  NULL, @ClientId, N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P10, NULL, @ClientId, N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P11, NULL, @ClientId, N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P12, NULL, @ClientId, N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P13, NULL, @ClientId, N'cover-brutalist-portfolio.jpg', N'/assets/client-home/brutalist-portfolio.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P14, NULL, @ClientId, N'cover-social-map-interface.jpg', N'/assets/client-home/social-map-interface.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P15, NULL, @ClientId, N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg', N'Brief', @Now, @By, 0),
(NEWID(), @P16, NULL, @ClientId, N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg', N'Brief', @Now, @By, 0);

/* Project members for assigned work */
INSERT INTO [marketplace].[ProjectMembers]
    ([Id], [ProjectId], [UserId], [RoleInProject], [AssignedByUserId], [AssignedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @P14, @Dev3, N'Mobile Lead', @ClientId, DATEADD(DAY, -10, @Now), @Now, @By, 0),
(NEWID(), @P14, @Dev5, N'QA', @ClientId, DATEADD(DAY, -10, @Now), @Now, @By, 0),
(NEWID(), @P15, @Dev1, N'Data Lead', @ClientId, DATEADD(DAY, -20, @Now), @Now, @By, 0),
(NEWID(), @P15, @Dev2, N'BI Analyst', @ClientId, DATEADD(DAY, -20, @Now), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Proposals                                                                  */
/* -------------------------------------------------------------------------- */

DECLARE @Prop1  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000001';
DECLARE @Prop2  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000002';
DECLARE @Prop3  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000003';
DECLARE @Prop4  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000004';
DECLARE @Prop5  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000005';
DECLARE @Prop6  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000006';
DECLARE @Prop7  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000007';
DECLARE @Prop8  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000008';
DECLARE @Prop9  UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000009';
DECLARE @Prop10 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000010';
DECLARE @Prop11 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000011';
DECLARE @Prop12 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000012';
DECLARE @Prop13 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000013';
DECLARE @Prop14 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000014';
DECLARE @Prop15 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000015';
DECLARE @Prop16 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000016';
DECLARE @Prop17 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000017';
DECLARE @Prop18 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-000000000018';

INSERT INTO [marketplace].[ProjectProposals]
(
    [Id], [ProjectId], [ApplicantType], [TeamId], [UserId], [CoverLetter],
    [ProposedBudget], [Status], [AppliedAt], [ResponseAt],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
/* Open — pending team + solo */
(@Prop1,  @P1,  N'Team', @TeamWeb,    NULL,  N'WebSquad can deliver the analytics dashboard with React + charts in 4 weeks.', 1400.00, N'Pending', DATEADD(HOUR, -20, @Now), NULL, @Now, @By, 0),
(@Prop2,  @P1,  N'User', NULL,         @Dev4, N'Solo proposal focused on role-based dashboards and performance.', 1250.00, N'Pending', DATEADD(HOUR, -18, @Now), NULL, @Now, @By, 0),
(@Prop3,  @P2,  N'Team', @TeamWeb,    NULL,  N'Fast Next.js landing with SEO and conversion-focused sections.', 520.00, N'Pending', DATEADD(HOUR, -16, @Now), NULL, @Now, @By, 0),
(@Prop4,  @P3,  N'Team', @TeamWeb,    NULL,  N'NestJS inventory API with auth, pagination, and Postgres.', 3800.00, N'Pending', DATEADD(HOUR, -15, @Now), NULL, @Now, @By, 0),
(@Prop5,  @P4,  N'Team', @TeamMobile, NULL,  N'MobileForge Flutter delivery tracker with realtime updates.', 8200.00, N'Pending', DATEADD(HOUR, -14, @Now), NULL, @Now, @By, 0),
(@Prop6,  @P4,  N'User', NULL,         @Dev3, N'Solo Flutter specialist available for the delivery app MVP.', 7000.00, N'Pending', DATEADD(HOUR, -13, @Now), NULL, @Now, @By, 0),
(@Prop7,  @P5,  N'Team', @TeamWeb,    NULL,  N'SaaS admin MVP with ASP.NET Core + React reporting.', 6000.00, N'Pending', DATEADD(HOUR, -12, @Now), NULL, @Now, @By, 0),
(@Prop8,  @P6,  N'Team', @TeamDesign, NULL,  N'PixelCraft fintech design system in Figma with tokens and components.', 2100.00, N'Pending', DATEADD(HOUR, -11, @Now), NULL, @Now, @By, 0),
(@Prop9,  @P7,  N'Team', @TeamCloud,  NULL,  N'CloudOps Azure CI/CD with Docker staging/production pipelines.', 2600.00, N'Pending', DATEADD(HOUR, -10, @Now), NULL, @Now, @By, 0),
(@Prop10, @P8,  N'Team', @TeamCloud,  NULL,  N'Playwright E2E suite covering auth, checkout, and regressions.', 1500.00, N'Pending', DATEADD(HOUR, -9, @Now), NULL, @Now, @By, 0),
(@Prop11, @P9,  N'Team', @TeamData,   NULL,  N'DataNest bilingual LLM support chatbot with LangChain.', 7200.00, N'Pending', DATEADD(HOUR, -8, @Now), NULL, @Now, @By, 0),
(@Prop12, @P10, N'Team', @TeamSecure, NULL,  N'SecureShield OWASP review, pentest, and remediation report.', 4200.00, N'Pending', DATEADD(HOUR, -7, @Now), NULL, @Now, @By, 0),
(@Prop13, @P11, N'Team', @TeamShop,   NULL,  N'ShopForge Shopify theme customization and Stripe checkout UX.', 28000.00, N'Pending', DATEADD(HOUR, -6, @Now), NULL, @Now, @By, 0),
(@Prop14, @P12, N'User', NULL,         @Dev2, N'Hourly Angular portal enhancements with RxJS expertise.', 1200.00, N'Pending', DATEADD(HOUR, -5, @Now), NULL, @Now, @By, 0),
/* In progress — accepted */
(@Prop15, @P14, N'Team', @TeamMobile, NULL,  N'Accepted: MobileForge owns the iOS fitness tracker delivery.', 8500.00, N'Accepted', DATEADD(DAY, -12, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -12, @Now), @By, 0),
(@Prop16, @P14, N'User', NULL,         @Dev5, N'Rejected alternate solo bid for the fitness tracker.', 7800.00, N'Rejected', DATEADD(DAY, -11, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -11, @Now), @By, 0),
/* Completed — accepted */
(@Prop17, @P15, N'Team', @TeamData,   NULL,  N'Accepted: DataNest delivered Power BI sales dashboards.', 1800.00, N'Accepted', DATEADD(DAY, -25, @Now), DATEADD(DAY, -23, @Now), DATEADD(DAY, -25, @Now), @By, 0),
/* Cancelled — withdrawn */
(@Prop18, @P16, N'Team', @TeamShop,   NULL,  N'Withdrawn after Magento marketplace was cancelled.', 12000.00, N'Withdrawn', DATEADD(DAY, -30, @Now), DATEADD(DAY, -28, @Now), DATEADD(DAY, -30, @Now), @By, 0);

COMMIT TRANSACTION;

SELECT N'seeded_ok' AS Result;
SELECT COUNT(*) AS FilterProjects
FROM [marketplace].[Projects]
WHERE [Id] LIKE '44444444-4444-4444-4444-%';
SELECT COUNT(*) AS Proposals
FROM [marketplace].[ProjectProposals]
WHERE [Id] LIKE '55555555-5555-5555-5555-%';
SELECT COUNT(*) AS CoverFiles
FROM [marketplace].[ProjectFiles]
WHERE [FileUrl] LIKE N'/assets/client-home/%';
SELECT [Name], [TeamCode]
FROM [teams].[Teams]
WHERE [Id] LIKE 'cccccccc-cccc-cccc-cccc-%'
ORDER BY [TeamCode];
GO
