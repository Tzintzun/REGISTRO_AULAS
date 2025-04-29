using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using System.Windows;
using Serilog;
using AulasSiencb2.Models;
using System.ComponentModel;
using System.Windows.Input;
using AulasSiencb2.Resources;
using System.Windows.Forms;
using System.Diagnostics;
using AulasSiencb2.Helpers;
using AulasSiencb2.Services;

namespace AulasSiencb2.ViewModel
{
    public class Service
    {
        public int pkProducto
        {
            get; set;
        }
        public int pkCostoProducto
        {
            get; set;
        }

        public Decimal costo
        {
            get; set;
        }

        public Decimal porcentajeImpuesto
        {
            get; set;
        }

        public Decimal subTotal;
        public Decimal ivaTotal;
        public int countPages;
        public string nombreProducto
        {
            get; set;
        }

        public Service(JsonObject obj)
        {
            try
            {
                pkProducto = (int)obj["PK_PRODUCTO"];
                pkCostoProducto = (int)obj["PK_COSTO_PRODUCTO"];
                nombreProducto = (string)obj["NOMBRE_PRODUCTO"];
                costo = (decimal)obj["COSTO"];
                porcentajeImpuesto = (decimal)obj["PORCENTAJE_IMPUESTO"];
            }catch(Exception e)
            {
                
                throw new ArgumentException("Error al cargar el servicio: ", e.Message);
               
            }
            
        }

    }
    class ServiceFormViewModel : INotifyPropertyChanged
    {
        private decimal _balance;
        private ApplicationState _state;
        private ScreenServiceViewModel _screen;
        private List<Service> _services;

        private string _conceptColor;
        private string _costColor;
        private int _countColor;
        private decimal _costTotal;

        private string _conceptBW;
        private string _costBW;
        private int _countBW;
        private decimal _costTotalBW;
        private bool _isCheckBW;
        private bool _isCheckColor;


        public string ConceptColor
        {
            get { return _conceptColor; }
            set
            {
                _conceptColor = value;
                OnPropertyChanged(nameof(ConceptColor));
            }
        }

        public string CostColor
        {
            get { return _costColor; }
            set
            {
                _costColor = value;
                OnPropertyChanged(nameof(CostColor));
            }
        }

        public int CountColor
        {
            get { return _countColor; }
            set
            {
                _countColor = value;
                OnPropertyChanged(nameof(CountColor));
            }
        }
        public decimal CostTotal
        {
            get { return _costTotal; }
            set
            {
                _costTotal = value;
                OnPropertyChanged(nameof(CostTotal));
            }
        }

        public bool IsCheckColor
        {
            get { return _isCheckColor; }
            set
            {
                _isCheckColor = value;
                OnPropertyChanged(nameof(IsCheckColor));
            }
        }

        public string ConceptBW
        {
            get { return _conceptBW; }
            set
            {
                _conceptBW = value;
                OnPropertyChanged(nameof(ConceptBW));
            }
        }

        public string CostBW
        {
            get { return _costBW; }
            set
            {
                _costBW = value;
                OnPropertyChanged(nameof(CostBW));
            }
        }

        public int CountBW
        {
            get { return _countBW; }
            set
            {
                
                _countBW = value;
                OnPropertyChanged(nameof(CountBW));
                CalculatePriceBW();
            }
        }

        public decimal CostTotalBW
        {
            get { return _costTotalBW; }
            set
            {
                _costTotalBW = value;
                OnPropertyChanged(nameof(CostTotalBW));
            }
        }

        public bool IsCheckBW
        {
            get { return _isCheckBW; }
            set
            {
                _isCheckBW = value;
                OnPropertyChanged(nameof(IsCheckBW));
            }
        }

        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }

        public List<Service> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged(nameof(Services));
            }
        }


        public ICommand CalculatePriceBWCommand { get; set; }
        public ICommand CalculatePriceColorCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand RequestServiceCommand { get; set; }

        public void LoadOptions()
        {
            foreach (Service service in _services)
            {
                if (service.nombreProducto.Contains("COLOR"))
                {
                    ConceptColor = service.nombreProducto;
                    CostColor = (service.costo * ((service.porcentajeImpuesto / 100) + 1)).ToString();
                    CountColor = 0;
                    CostTotal = 0;
                }
                if(service.nombreProducto.Contains("BLANCO Y NEGRO"))
                {
                    ConceptBW = service.nombreProducto;
                    CostBW = (service.costo * ((service.porcentajeImpuesto/100)+1)).ToString();
                    CountBW = 0;
                    CostTotalBW = 0;
                }
            }
        }

        private void CalculatePriceBW()
        {
            foreach (Service service in _services) {
                if (service.nombreProducto.Contains("BLANCO Y NEGRO"))
                {
                    CostTotalBW = (service.costo * Convert.ToDecimal(CountBW)) * ((service.porcentajeImpuesto / 100)+1);
                    service.subTotal = service.costo * Convert.ToDecimal(CountBW);
                    service.ivaTotal = CostTotalBW - service.subTotal;
                    service.countPages = CountBW;
                }
            }
        }

        private async void GetBalance()
        {
            ApiService api = new ApiService();

            decimal balance = await api.GetBalance(_state.Session.User.PkUsuario, _state.Session.User.PkCategoria);
            Debug.WriteLine(balance);
            if (balance == null || balance < 0)
            {
                //System.Windows.MessageBox.Show("Error al cargar el saldo");
                return;
            }
            Balance = balance;
        }
        private decimal GetBWPricePage()
        {
            foreach (Service service in _services)
            {
                if (service.nombreProducto.Contains("BLANCO Y NEGRO"))
                {
                    return (service.costo);
                }
            }
            return 0.5m;
        }
        private decimal GetPercentageIVAColor()
        {
            foreach (Service service in _services)
            {
                if (service.nombreProducto.Contains("BLANCO Y NEGRO"))
                {
                    return (service.porcentajeImpuesto / 100) +1;
                }
            }
            return 1.16m;
        }
        private decimal GetColorPricePage()
        {
            foreach (Service service in _services)
            {
                if (service.nombreProducto.Contains("COLOR"))
                {
                    return (service.costo);
                }
            }
            return 10.5m;
        }
        private async Task CalculatePriceColor()
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos Pdf (*.pdf)|*.pdf";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _screen.ShowLoading();
                Debug.WriteLine(openFileDialog.FileName);
                JsonObject values = new JsonObject();
                await Task.Run(() =>
                {
                    values = PrintUtils.CaculatePercentageColor(openFileDialog.FileName);
                });
                if (!((bool)values["success"]))
                {
                    _screen.ShowForm();
                    System.Windows.MessageBox.Show($"Lo sentimos, no se pudo calcular el precio para {openFileDialog.FileName}");
                    return;
                }
                JsonArray arrayPages = (JsonArray)values["pages"];
                decimal grossPrice = 0;
                foreach (JsonObject page in arrayPages) {
                    decimal percentage = (decimal)page["percentage"];
                    decimal grosPricePage = GetBWPricePage();
                    if(percentage>0.0m && percentage < 10.0m)
                    {
                        grosPricePage = 2.5m;
                    }
                    if(percentage >= 10.0m && percentage < 50.0m)
                    {
                        grosPricePage = 4.0m;
                    }
                    if(percentage >= 50.0m && percentage < 75.0m)
                    {
                        grosPricePage = 6.0m;
                    }
                    if (percentage >= 75.0m)
                    {
                        grosPricePage = percentage * GetColorPricePage();
                    }
                    grossPrice = Decimal.Add(grossPrice, grosPricePage);
                    Debug.WriteLine(page.ToString());
                }
                CountColor = arrayPages.Count;
                CostTotal = grossPrice * GetPercentageIVAColor();
                _screen.ShowForm();

                foreach (Service service in _services)
                {
                    if (service.nombreProducto.Contains("COLOR"))
                    {
                         service.subTotal = grossPrice;
                        service.ivaTotal = CostTotal - grossPrice;
                        service.countPages = CountColor;

                    }
                }
                //Debug.WriteLine(values.ToString());
                Debug.WriteLine($"Numero de paginas: {arrayPages.Count}");
            }

        }

        private async Task RequestService()
        {
            try
            {
                
                Service service = null;
                if (IsCheckBW)
                {
                    if (CountBW <= 0)
                    {
                        System.Windows.MessageBox.Show("El numero de paginas debe ser mayor a 0");
                        _screen.ShowForm();
                        return;
                    }
                    if (Balance < CostTotalBW || CostTotalBW <= 0.0m)
                    {
                        System.Windows.MessageBox.Show("Saldo insuficiente");
                        _screen.ShowForm();
                        return;
                    }
                    foreach (Service s in _services)
                    {
                        if (s.nombreProducto.Contains("BLANCO Y NEGRO"))
                        {
                            service = s;
                        }
                    }
                }

                if (IsCheckColor)
                {
                    if (CountColor <= 0)
                    {
                        System.Windows.MessageBox.Show("El numero de paginas debe ser mayor a 0");
                        _screen.ShowForm();
                        return;
                    }
                    if (Balance < CostTotal || CostTotal <= 0.0m)
                    {
                        System.Windows.MessageBox.Show("Saldo insuficiente");
                        _screen.ShowForm();
                        return;
                    }
                    foreach (Service s in _services)
                    {
                        if (s.nombreProducto.Contains("COLOR"))
                        {
                            service = s;
                        }
                    }
                }
                if (service == null)
                {
                    System.Windows.MessageBox.Show("Por favor selecciona un servicio");
                    _screen.ShowForm();
                    return;
                }
                _screen.ShowLoading();
                ApiService apiService = new ApiService();
                JsonObject response = await apiService.RequestService(_state.Session, service);
                if (((bool)response["success"]))
                {
                    ClearData();
                    GetBalance();

                }
                _screen.ShowForm();
                System.Windows.MessageBox.Show(response["message"]);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _screen.ShowForm();
                System.Windows.MessageBox.Show("Error al solicitar el servicio");

            }
            finally
            {
                _screen.ShowForm();
            }
        }

        private void ClearData()
        {
            foreach (Service s in _services)
            {
                s.countPages = 0;
                s.ivaTotal = 0;
                s.subTotal = 0;
            }
            CountBW = 0;
            CountColor = 0;
            CostTotal = 0;
            CostTotalBW = 0;
            IsCheckBW = false;
            IsCheckColor = false;
        }

        private async Task CloseWindow()
        {
            _screen.CloseWindow(true);
        }

        public ServiceFormViewModel(ApplicationState state, ScreenServiceViewModel screen)
        {
            _state = state;
            _screen = screen;
            Balance = 0;
            CalculatePriceBWCommand = new SimpleCommand(() => CalculatePriceBW());
            CalculatePriceColorCommand = new SimpleCommand(async () =>  await CalculatePriceColor());
            CloseWindowCommand = new SimpleCommand(async () => await CloseWindow());
            RequestServiceCommand = new SimpleCommand(async () => await RequestService());
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
