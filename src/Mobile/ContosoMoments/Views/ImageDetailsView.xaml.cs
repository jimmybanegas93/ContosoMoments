﻿using ContosoMoments.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices.Files;
using System.IO;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using PCLStorage;

namespace ContosoMoments.Views
{
    public partial class ImageDetailsView : ContentPage
    {
        public ImageDetailsView()
        {
            InitializeComponent();

            var tapLikeImage = new TapGestureRecognizer();
            tapLikeImage.Tapped += OnLike;
            imgLike.GestureRecognizers.Add(tapLikeImage);

            var tapSettingsImage = new TapGestureRecognizer();
            tapSettingsImage.Tapped += OnSettings;
            imgSettings.GestureRecognizers.Add(tapSettingsImage);
        }

        public async void OnLike(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;

            try {
                await vm.LikeImageAsync();
            }
            catch (Exception) {
                await DisplayAlert("Error", "'Like' functionality is not available at the moment. Please try again later", "OK");
            }
        }

        public async void OnSettings(object sender, EventArgs e)
        {
            ImageDetailsViewModel vm = this.BindingContext as ImageDetailsViewModel;
            await Navigation.PushModalAsync(new SettingView(App.Current as App));
        }

        public async void OnOpenImage(object sender, EventArgs args)
        {
            var button = (Button)sender;
            string imageSize = button.CommandParameter.ToString();

            var vm = this.BindingContext as ImageDetailsViewModel;

            IFileSyncContext context = App.MobileService.GetFileSyncContext();

            var recordFiles = await context.MobileServiceFilesClient.GetFilesAsync(App.Instance.imageTableSync.TableName, vm.Image.Id);
            var file = recordFiles.First(f => f.StoreUri.Contains(imageSize));

            if (file != null) {
                var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, imageSize + "-" + file.Name);
                await App.Instance.imageTableSync.DownloadFileAsync(file, path);
                await Navigation.PushAsync(CreateDetailsPage(path));

                // delete the file
                var fileRef = await FileSystem.Current.LocalStorage.GetFileAsync(path);
                await fileRef.DeleteAsync();
            }
        }

        private static MobileServiceFile GetFileReference(Models.Image image, string param)
        {
            var toDownload = new MobileServiceFile(image.File.Id, image.File.Name, image.File.TableName, image.File.ParentId) {
                StoreUri = image.File.StoreUri.Replace("lg", param)
            };

            return toDownload;
        }

        private static ContentPage CreateDetailsPage(string uri) 
        {
            var imagePage = new ContentPage {
                Content = new StackLayout() {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new Xamarin.Forms.Image {
                            Aspect = Aspect.AspectFill,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            Source = ImageSource.FromFile(uri)
                       }
                   }
                }
            };

            return imagePage;
        }
    }
}
