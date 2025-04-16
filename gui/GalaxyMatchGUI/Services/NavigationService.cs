using System;
using System.Collections.Generic;
using Avalonia.Controls;
using GalaxyMatchGUI.ViewModels;

namespace GalaxyMatchGUI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Func<ContentControl> _getContentControl;
        private readonly Dictionary<Type, Type> _viewModelToViewMap;
        private readonly Stack<ViewModelBase> _navigationHistory = new Stack<ViewModelBase>();
        
        public NavigationService(Func<ContentControl> getContentControl)
        {
            _getContentControl = getContentControl;
            _viewModelToViewMap = new Dictionary<Type, Type>();
        }
        
        public void RegisterView<TViewModel, TView>() where TViewModel : ViewModelBase where TView : Control
        {
            var viewModelType = typeof(TViewModel);
            var viewType = typeof(TView);
            
            if (_viewModelToViewMap.ContainsKey(viewModelType))
            {
                _viewModelToViewMap[viewModelType] = viewType;
            }
            else
            {
                _viewModelToViewMap.Add(viewModelType, viewType);
            }
        }
        
        public void NavigateTo<T>(T? viewModel = null) where T : ViewModelBase
        {
            var viewModelType = typeof(T);
            
            if (!_viewModelToViewMap.ContainsKey(viewModelType))
            {
                throw new InvalidOperationException($"No view registered for ViewModel type {viewModelType.Name}");
            }
            
            var view = (Control)Activator.CreateInstance(_viewModelToViewMap[viewModelType])!;
            
            var vm = viewModel ?? (T)Activator.CreateInstance(viewModelType)!;
            view.DataContext = vm;
            
            var contentControl = _getContentControl();
            
            if (contentControl.Content != null && contentControl.DataContext is ViewModelBase currentViewModel)
            {
                _navigationHistory.Push(currentViewModel);
            }
            
            contentControl.Content = view;
        }
        
        public bool NavigateBack()
        {
            var contentControl = _getContentControl();
            
            if (_navigationHistory.Count == 0)
            {
                return false;
            }
            
            var previousViewModel = _navigationHistory.Pop();
            var viewModelType = previousViewModel.GetType();
            
            if (!_viewModelToViewMap.ContainsKey(viewModelType))
            {
                return false;
            }
            
            var view = (Control)Activator.CreateInstance(_viewModelToViewMap[viewModelType])!;
            view.DataContext = previousViewModel;
            contentControl.Content = view;
            
            return true;
        }
    }
}