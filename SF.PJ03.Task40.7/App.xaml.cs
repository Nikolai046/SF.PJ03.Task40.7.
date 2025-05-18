using SF.PJ03.Task40._7_.Pages;

namespace SF.PJ03.Task40._7_
{
    public partial class App : Application
    {
        // Инициализирует новый экземпляр класса App и устанавливает AppShell как главную страницу.
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}