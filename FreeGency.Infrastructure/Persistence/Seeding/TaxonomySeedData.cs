namespace FreeGency.Infrastructure.Persistence.Seeding;

internal static class TaxonomySeedData
{
    internal const string SeedUser = "system";

    internal static readonly IReadOnlyList<CategoryDefinition> Categories =
    [
        new(
            "web",
            "Web Development",
            "تطوير الويب",
            [
                "Frontend Development",
                "Backend Development",
                "Full Stack Development",
                "API Development",
                "Progressive Web Apps (PWA)",
                "WordPress / CMS Development",
                "Landing Pages & Marketing Sites",
                "Real-time Systems"
            ],
            SplitSkills("JavaScript · TypeScript · React · Next.js · Angular · Vue.js · Nuxt.js · Svelte · HTML · CSS · Tailwind CSS · Bootstrap · Redux · RxJS · Node.js · Express · NestJS · REST · GraphQL · WebSockets · PostgreSQL · MongoDB · Redis · WordPress · Vite · Webpack · Git")),
        new(
            "mobile",
            "Mobile Development",
            "تطوير الموبايل",
            [
                "iOS Development",
                "Android Development",
                "Cross-Platform Mobile Development",
                "Mobile UI Implementation",
                "App Store / Play Store Deployment",
                "API Development",
                "Real-time Systems"
            ],
            SplitSkills("Swift · SwiftUI · Kotlin · Jetpack Compose · Flutter · Dart · React Native · .NET MAUI · Firebase · REST · GraphQL · WebSockets · SQLite · Appium · Git · Android SDK · iOS SDK")),
        new(
            "saas",
            "Custom Software / SaaS",
            "برمجيات مخصصة / SaaS",
            [
                "SaaS Product Development",
                "Internal Tools / Admin Panels",
                "ERP / CRM Customization",
                "System Integration",
                "MVP / Prototype Development",
                "Microservices Architecture",
                "Legacy System Modernization",
                "Plugin / Extension Development",
                "API Development"
            ],
            SplitSkills("C# · .NET · ASP.NET Core · Node.js · NestJS · Python · Django · FastAPI · Java · Spring Boot · PostgreSQL · SQL Server · MongoDB · Redis · Docker · Kubernetes · AWS · Azure · REST · GraphQL · gRPC · Microservices · System Design · Git · Agile · Scrum")),
        new(
            "uiux",
            "UI/UX for Software",
            "تصميم واجهات البرمجيات",
            [
                "Product UI Design",
                "UX Research & Wireframing",
                "Design Systems",
                "Prototyping",
                "Mobile App Design",
                "Dashboard / SaaS Design",
                "Design-to-Code Handoff"
            ],
            SplitSkills("Figma · Adobe XD · Sketch · Framer · Zeplin · User Research · Wireframing · Prototyping · Design Systems · UI Design · UX Design · Accessibility · Tailwind CSS · HTML · CSS · React · Angular")),
        new(
            "backend",
            "Backend & APIs",
            "الباك اند وواجهات البرمجة",
            [
                "Backend Development",
                "API Development",
                "Microservices Architecture",
                "Real-time Systems",
                "System Integration",
                "Legacy System Modernization"
            ],
            SplitSkills("Node.js · Express · NestJS · C# · .NET · ASP.NET Core · Java · Spring Boot · Python · Django · FastAPI · Go · PHP · Laravel · Ruby · Ruby on Rails · REST · GraphQL · gRPC · WebSockets · SignalR · PostgreSQL · MySQL · SQL Server · MongoDB · Redis · Elasticsearch · OAuth · JWT · Docker · Git · System Design")),
        new(
            "devops",
            "DevOps & Cloud",
            "ديف أوبس والسحابة",
            [
                "CI/CD Pipelines",
                "Cloud Infrastructure",
                "Docker & Kubernetes",
                "Infrastructure as Code",
                "Monitoring & Logging",
                "Serverless Architecture",
                "Cloud Migration",
                "MLOps"
            ],
            SplitSkills("Docker · Kubernetes · AWS · Azure · GCP · Terraform · Ansible · Jenkins · GitHub Actions · GitLab CI · Helm · Linux · Nginx · Serverless · Prometheus · Grafana · ELK Stack · Bash · Python · Git · CI/CD")),
        new(
            "qa",
            "QA & Testing",
            "ضمان الجودة والاختبار",
            [
                "Manual Testing",
                "Automated Testing",
                "API Testing",
                "Performance Testing",
                "Mobile App Testing",
                "Test Strategy & QA Process",
                "Security Testing"
            ],
            SplitSkills("Selenium · Cypress · Playwright · Jest · xUnit · JUnit · Postman · k6 · Appium · Manual Testing · Test Automation · API Testing · Performance Testing · Regression Testing · Bug Reporting · Agile · Scrum · Git")),
        new(
            "dataai",
            "Data & AI",
            "البيانات والذكاء الاصطناعي",
            [
                "Data Engineering / ETL",
                "Data Analysis & BI",
                "Machine Learning",
                "NLP / LLMs / Chatbots",
                "Computer Vision",
                "Recommendation Systems",
                "MLOps"
            ],
            SplitSkills("Python · SQL · Pandas · NumPy · scikit-learn · TensorFlow · PyTorch · Spark · Airflow · PostgreSQL · MongoDB · OpenAI API · LangChain · Hugging Face · Power BI · Tableau · ETL · Machine Learning · NLP · Computer Vision · MLOps · Git")),
        new(
            "security",
            "Cybersecurity",
            "الأمن السيبراني",
            [
                "Application Security",
                "Penetration Testing",
                "Secure Code Review",
                "Auth & Identity Security",
                "Cloud Security",
                "Vulnerability Assessment",
                "Security Testing"
            ],
            SplitSkills("OWASP · OAuth · JWT · SSL/TLS · IAM · Penetration Testing · Burp Suite · Secure Code Review · Application Security · Cloud Security · Vulnerability Assessment · Identity Management · Firewall · Linux · Python · Git")),
        new(
            "ecommerce",
            "E-commerce Development",
            "تطوير التجارة الإلكترونية",
            [
                "Shopify Development",
                "WooCommerce Development",
                "Custom E-commerce Platforms",
                "Payment Gateway Integration",
                "Inventory & Order Systems",
                "Marketplace Platforms",
                "WordPress / CMS Development"
            ],
            SplitSkills("Shopify · WooCommerce · Magento · WordPress · Stripe · PayPal · JavaScript · TypeScript · React · Next.js · PHP · Laravel · Node.js · PostgreSQL · MySQL · REST · Payment Integration · Inventory Management · Git"))
    ];

    internal static readonly IReadOnlyDictionary<string, string> SpecialtyNamesAr =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Frontend Development"] = "تطوير الواجهات الأمامية",
            ["Backend Development"] = "تطوير الباك اند",
            ["Full Stack Development"] = "تطوير Full Stack",
            ["API Development"] = "تطوير واجهات البرمجة",
            ["Progressive Web Apps (PWA)"] = "تطبيقات الويب التقدمية",
            ["WordPress / CMS Development"] = "تطوير WordPress / CMS",
            ["Landing Pages & Marketing Sites"] = "صفحات هبوط ومواقع تسويقية",
            ["Real-time Systems"] = "أنظمة Real-time",
            ["iOS Development"] = "تطوير iOS",
            ["Android Development"] = "تطوير Android",
            ["Cross-Platform Mobile Development"] = "تطوير موبايل Cross-Platform",
            ["Mobile UI Implementation"] = "تنفيذ واجهات الموبايل",
            ["App Store / Play Store Deployment"] = "نشر على App Store / Play Store",
            ["SaaS Product Development"] = "تطوير منتجات SaaS",
            ["Internal Tools / Admin Panels"] = "أدوات داخلية / لوحات تحكم",
            ["ERP / CRM Customization"] = "تخصيص ERP / CRM",
            ["System Integration"] = "تكامل الأنظمة",
            ["MVP / Prototype Development"] = "تطوير MVP / Prototype",
            ["Microservices Architecture"] = "هندسة Microservices",
            ["Legacy System Modernization"] = "تحديث الأنظمة القديمة",
            ["Plugin / Extension Development"] = "تطوير Plugins / Extensions",
            ["Product UI Design"] = "تصميم واجهات المنتج",
            ["UX Research & Wireframing"] = "بحث UX و Wireframes",
            ["Design Systems"] = "أنظمة التصميم",
            ["Prototyping"] = "النماذج الأولية",
            ["Mobile App Design"] = "تصميم تطبيقات الموبايل",
            ["Dashboard / SaaS Design"] = "تصميم Dashboard / SaaS",
            ["Design-to-Code Handoff"] = "تسليم التصميم للكود",
            ["CI/CD Pipelines"] = "خطوط CI/CD",
            ["Cloud Infrastructure"] = "بنية سحابية",
            ["Docker & Kubernetes"] = "Docker و Kubernetes",
            ["Infrastructure as Code"] = "Infrastructure as Code",
            ["Monitoring & Logging"] = "المراقبة والتسجيل",
            ["Serverless Architecture"] = "هندسة Serverless",
            ["Cloud Migration"] = "الترحيل للسحابة",
            ["MLOps"] = "MLOps",
            ["Manual Testing"] = "اختبار يدوي",
            ["Automated Testing"] = "اختبار آلي",
            ["API Testing"] = "اختبار APIs",
            ["Performance Testing"] = "اختبار الأداء",
            ["Mobile App Testing"] = "اختبار تطبيقات الموبايل",
            ["Test Strategy & QA Process"] = "استراتيجية الاختبار و QA",
            ["Security Testing"] = "اختبار الأمان",
            ["Data Engineering / ETL"] = "هندسة البيانات / ETL",
            ["Data Analysis & BI"] = "تحليل البيانات / BI",
            ["Machine Learning"] = "تعلم الآلة",
            ["NLP / LLMs / Chatbots"] = "NLP / LLMs / Chatbots",
            ["Computer Vision"] = "رؤية حاسوبية",
            ["Recommendation Systems"] = "أنظمة التوصية",
            ["Application Security"] = "أمان التطبيقات",
            ["Penetration Testing"] = "اختبار الاختراق",
            ["Secure Code Review"] = "مراجعة الكود الأمنية",
            ["Auth & Identity Security"] = "أمان المصادقة والهوية",
            ["Cloud Security"] = "أمان السحابة",
            ["Vulnerability Assessment"] = "تقييم الثغرات",
            ["Shopify Development"] = "تطوير Shopify",
            ["WooCommerce Development"] = "تطوير WooCommerce",
            ["Custom E-commerce Platforms"] = "منصات تجارة إلكترونية مخصصة",
            ["Payment Gateway Integration"] = "تكامل بوابات الدفع",
            ["Inventory & Order Systems"] = "أنظمة المخزون والطلبات",
            ["Marketplace Platforms"] = "منصات Marketplace"
        };

    internal static Guid CategoryId(string key) =>
        Guid.Parse($"11111111-1111-1111-1111-{CategoryKeyToHex(key)}");

    internal static Guid EntityId(string prefix, int index) =>
        Guid.Parse($"{prefix}-{index:D12}");

    private static string CategoryKeyToHex(string key) => key switch
    {
        "web" => "000000000001",
        "mobile" => "000000000002",
        "saas" => "000000000003",
        "uiux" => "000000000004",
        "backend" => "000000000005",
        "devops" => "000000000006",
        "qa" => "000000000007",
        "dataai" => "000000000008",
        "security" => "000000000009",
        "ecommerce" => "00000000000a",
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown category key.")
    };

    private static string[] SplitSkills(string raw) =>
        raw.Split('·', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    internal sealed record CategoryDefinition(
        string Key,
        string NameEn,
        string NameAr,
        string[] Specialties,
        string[] Skills);
}
