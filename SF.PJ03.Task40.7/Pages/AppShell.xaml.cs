namespace SF.PJ03.Task40._7_.Pages
{
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Класс оболочки приложения, определяющий основную структуру навигации. 
        /// Управляет отображением страницы входа или регистрации в зависимости от наличия сохраненного PIN-кода.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("GalleryPage", typeof(GalleryPage));


            // Выбор страницы в зависимости от наличия сохраненного PIN-кода
            var storedHash = SecureStorage.Default.GetAsync("UserPin").Result;

            var content = new ShellContent
            {
                ContentTemplate = new DataTemplate(() => storedHash == null ? new SignUpPage() : new SignInPage()),
            };

            this.Items.Add(content);
        }
    }
}