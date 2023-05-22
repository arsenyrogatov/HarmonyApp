using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    public interface IWindowService
    {
        public void ShowWindow<T>(object dataContext) where T : Window, new();
    }

    public class WindowService : IWindowService
    {
        public void ShowWindow<T>(object dataContext) where T : Window, new()
        {
            var window = new T();

            window.Show();
        }
    }
}
