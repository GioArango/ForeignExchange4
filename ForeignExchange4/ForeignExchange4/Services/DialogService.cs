namespace ForeignExchange4.Services
{
    using ForeignExchange4.Helpers;
    using System.Threading.Tasks;
    using Xamarin.Forms;

    public class DialogService
    {
        public async Task ShowMessage(string tittle, string message)
        {
            await Application.Current.MainPage.DisplayAlert(
                tittle,
                message,
                Lenguages.Accept);
        }
    }
}
