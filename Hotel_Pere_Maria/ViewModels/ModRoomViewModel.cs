using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hotel_Pere_Maria.ViewModels
{
    public sealed class ExtraServicePickViewModel : BaseViewModel
    {
        public string ServiceId { get; }
        public string Name { get; }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public ExtraServicePickViewModel(string serviceId, string name, bool initial)
        {
            ServiceId = serviceId;
            Name = name;
            _isSelected = initial;
        }
    }

    public class ModRoomViewModel : BaseViewModel
    {
        private string _roomId;
        private string _type;
        private string _price;
        private int _maxOccupancy;
        private string _rate;
        private string _description;
        private bool _isOperational;
        private bool _isRoomIdReadOnly;
        private bool _offerActive;
        private string _offerPercent = "0";
        private string _newImageUrl = "";
        private string _newServiceName = "";

        public string RoomId
        {
            get => _roomId;
            set
            {
                _roomId = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
                UpdateMaxOccupancy();
            }
        }

        public string Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public int MaxOccupancy
        {
            get => _maxOccupancy;
            set
            {
                _maxOccupancy = value;
                OnPropertyChanged();
            }
        }

        public string Rate
        {
            get => _rate;
            set
            {
                _rate = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public bool IsOperational
        {
            get => _isOperational;
            set
            {
                _isOperational = value;
                OnPropertyChanged();
            }
        }

        public bool OfferActive
        {
            get => _offerActive;
            set
            {
                _offerActive = value;
                OnPropertyChanged();
            }
        }

        public string OfferPercent
        {
            get => _offerPercent;
            set
            {
                _offerPercent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ImageUrls { get; } = new ObservableCollection<string>();

        public string NewImageUrl
        {
            get => _newImageUrl;
            set
            {
                _newImageUrl = value;
                OnPropertyChanged();
            }
        }

        public string NewServiceName
        {
            get => _newServiceName;
            set
            {
                _newServiceName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ExtraServicePickViewModel> ServicePicks { get; } =
            new ObservableCollection<ExtraServicePickViewModel>();

        public bool IsRoomIdReadOnly
        {
            get => _isRoomIdReadOnly;
            set
            {
                _isRoomIdReadOnly = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand CreateCatalogServiceCommand { get; }

        private readonly bool _isCreate;
        private readonly bool _employeeRateReadonly;
        private readonly double _lockedRate;

        /// <summary>Empleado: valoraci?n (rate) solo lectura; admin puede editar.</summary>
        public bool IsRateReadOnly => _employeeRateReadonly;

        public ModRoomViewModel(Room room, bool isCreate)
        {
            _isCreate = isCreate;
            _employeeRateReadonly = Session.User?.role == "employee";
            _lockedRate = room.Rate;
            IsRoomIdReadOnly = !isCreate;

            RoomId = room.RoomId ?? "";
            Type = (room.Type ?? "").Trim();
            Price = room.PricePerNight > 0 ? room.PricePerNight.ToString(CultureInfo.InvariantCulture) : "";
            MaxOccupancy = room.MaxOccupancy > 0 ? room.MaxOccupancy : 1;
            Rate = room.Rate.ToString(CultureInfo.InvariantCulture);
            Description = room.Description ?? "";
            IsOperational = room.IsOperational;
            OfferActive = room.OfferActive;
            OfferPercent =
                room.OfferPercent > 0 ? room.OfferPercent.ToString(CultureInfo.InvariantCulture) : "0";

            if (room.Images != null && room.Images.Count > 0)
            {
                foreach (var u in room.Images)
                {
                    if (!string.IsNullOrWhiteSpace(u))
                        ImageUrls.Add(u.Trim());
                }
            }
            else if (!string.IsNullOrWhiteSpace(room.Image))
            {
                foreach (var u in room.Image.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    ImageUrls.Add(u);
            }

            SaveCommand = new RelayCommand(() => _ = SaveAsync());
            AddImageCommand = new RelayCommand(AddImage);
            RemoveImageCommand = new RelayCommand<string>(RemoveImage);
            CreateCatalogServiceCommand = new RelayCommand(() => _ = CreateCatalogServiceAsync());

            _ = LoadCatalogAsync(room);
        }

        private void AddImage()
        {
            var u = (NewImageUrl ?? "").Trim();
            if (string.IsNullOrWhiteSpace(u))
                return;
            ImageUrls.Add(u);
            NewImageUrl = "";
        }

        private void RemoveImage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;
            var ix = ImageUrls.IndexOf(url);
            if (ix >= 0)
                ImageUrls.RemoveAt(ix);
        }

        private async Task CreateCatalogServiceAsync()
        {
            try
            {
                var n = (NewServiceName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(n))
                {
                    MessageBox.Show("Escribe el nombre del nuevo servicio.", "Validaci?n", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await ExtraServiceCatalogService.CreateAsync(n);
                NewServiceName = "";
                var list = await ExtraServiceCatalogService.ListAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServicePicks.Clear();
                    foreach (var e in list)
                        ServicePicks.Add(new ExtraServicePickViewModel(e.ServiceId, e.Name, false));
                });
                MessageBox.Show("Servicio creado. M?rcalo en la lista para asignarlo a esta habitaci?n.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCatalogAsync(Room room)
        {
            try
            {
                var list = await ExtraServiceCatalogService.ListAsync();
                var selected = room.ExtraServices ?? new List<string>();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServicePicks.Clear();
                    foreach (var e in list)
                    {
                        var on = selected.Contains(e.ServiceId);
                        ServicePicks.Add(new ExtraServicePickViewModel(e.ServiceId, e.Name, on));
                    }
                });
            }
            catch
            {
                /* sin cat?logo la UI sigue usable */
            }
        }

        private void UpdateMaxOccupancy()
        {
            MaxOccupancy = Type switch
            {
                "Individual" => 1,
                "Doble" => 2,
                "Suite" => 4,
                _ => MaxOccupancy,
            };
        }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RoomId))
                {
                    MessageBox.Show("El Room ID es obligatorio.", "Validaci?n", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Type))
                {
                    MessageBox.Show("Debes seleccionar un tipo de habitaci?n.", "Validaci?n", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Description))
                {
                    MessageBox.Show("La descripci?n es obligatoria.", "Validaci?n", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(Price?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double priceValue) || priceValue <= 0)
                {
                    MessageBox.Show("El precio debe ser un n?mero v?lido mayor que 0.", "Validaci?n", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double rateValue;
                if (_employeeRateReadonly)
                    rateValue = _lockedRate;
                else if (!double.TryParse(Rate?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out rateValue))
                    rateValue = 0;

                if (!double.TryParse(OfferPercent?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double offerPct))
                    offerPct = 0;
                offerPct = Math.Max(0, Math.Min(100, offerPct));

                var imgs = ImageUrls.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
                var joinedImage = imgs.Count > 0 ? string.Join(",", imgs) : "";

                var extraIds = ServicePicks.Where(s => s.IsSelected).Select(s => s.ServiceId).ToList();

                var payload = new Dictionary<string, object?>
                {
                    ["room_id"] = RoomId,
                    ["type"] = Type,
                    ["description"] = Description,
                    ["price_per_night"] = priceValue,
                    ["rate"] = rateValue,
                    ["max_occupancy"] = MaxOccupancy,
                    ["isOperational"] = IsOperational,
                    ["extra_services"] = extraIds,
                    ["offer_active"] = OfferActive,
                    ["offer_percent"] = offerPct,
                };

                if (imgs.Count > 0)
                {
                    payload["images"] = imgs;
                    payload["image"] = joinedImage;
                }
                else if (_isCreate)
                {
                    payload["image"] = "";
                }

                if (_isCreate)
                    await RoomService.CreateRoomAsync(payload);
                else
                    await RoomService.UpdateRoomAsync(payload);

                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.DialogResult = true;
                        win.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la habitaci?n:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
