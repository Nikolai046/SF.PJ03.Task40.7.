namespace SF.PJ03.Task40._7_.Pages
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            // Удаление
            //SecureStorage.Default.Remove("UserPin");

            InitializeComponent();

            Routing.RegisterRoute("GalleryPage", typeof(GalleryPage));

            var storedHash = SecureStorage.Default.GetAsync("UserPin").Result;

            var content = new ShellContent
            {
                ContentTemplate = new DataTemplate(() => storedHash == null ? new SignUpPage() : new SignInPage()),
            };

            this.Items.Add(content);

        }
    }
}
