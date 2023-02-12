﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ReorderableCollectionView.Forms
{
    public class ReorderableCollectionView : CollectionView
    {
        INotifyCollectionChanged _notifyCollection;
        IItemsLayout _itemsLayout;

        public static readonly BindableProperty ReorderStartedCommandProperty = BindableProperty.Create("ReorderStartedCommand", typeof(ICommand), typeof(ReorderableCollectionView));
        public ICommand ReorderStartedCommand
        {
            get { return (ICommand)GetValue(ReorderStartedCommandProperty); }
            set { SetValue(ReorderStartedCommandProperty, value); }
        }

        public static readonly BindableProperty ReorderCompletedCommandProperty = BindableProperty.Create("ReorderCompletedCommand", typeof(ICommand), typeof(ReorderableCollectionView));
        public ICommand ReorderCompletedCommand
        {
            get { return (ICommand)GetValue(ReorderCompletedCommandProperty); }
            set { SetValue(ReorderCompletedCommandProperty, value); }
        }

        public static readonly BindableProperty CanMixGroupsProperty = BindableProperty.Create("CanMixGroups", typeof(bool), typeof(ReorderableCollectionView), false);
        public bool CanMixGroups
        {
            get { return (bool)GetValue(CanMixGroupsProperty); }
            set { SetValue(CanMixGroupsProperty, value); }
        }

        public static readonly BindableProperty CanReorderItemsProperty = BindableProperty.Create("CanReorderItems", typeof(bool), typeof(ReorderableCollectionView), false);
        public bool CanReorderItems
        {
            get { return (bool)GetValue(CanReorderItemsProperty); }
            set { SetValue(CanReorderItemsProperty, value); }
        }

        public event EventHandler ReorderStarted;
        public event EventHandler ReorderCompleted;

        void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsLayout is VariableSpanGridItemsLayout)
            {
                if (Device.IsInvokeRequired)
                {
                    Device.BeginInvokeOnMainThread(() => InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged));
                }
            }
        }

        void HandleLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ItemsLayout is VariableSpanGridItemsLayout)
            {
                InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
            }
        }

        protected virtual void OnItemsLayoutChanged()
        {
            if (_itemsLayout != null)
            {
                _itemsLayout.PropertyChanged -= HandleLayoutPropertyChanged;
            }
            _itemsLayout = ItemsLayout;
            if (_itemsLayout != null)
            {
                _itemsLayout.PropertyChanged += HandleLayoutPropertyChanged;
            }

            if (ItemsLayout is VariableSpanGridItemsLayout)
            {
                InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
            }
        }

        protected virtual void OnItemsSourceChanged()
        {
            if (_notifyCollection != null)
            {
                _notifyCollection.CollectionChanged -= HandleCollectionChanged;
            }
            _notifyCollection = ItemsSource as INotifyCollectionChanged;
            if (_notifyCollection != null)
            {
                _notifyCollection.CollectionChanged += HandleCollectionChanged;
            }

            if (ItemsLayout is VariableSpanGridItemsLayout)
            {
                InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (ItemsLayout is VariableSpanGridItemsLayout)
            {
#pragma warning disable 0618 // retain until OnSizeRequest removed
                return OnSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 0618
            }
            else
            {
                return base.OnMeasure(widthConstraint, heightConstraint);
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ItemsLayoutProperty.PropertyName)
            {
                OnItemsLayoutChanged();
            }
            else if (propertyName == ItemsSourceProperty.PropertyName)
            {
                OnItemsSourceChanged();
            }

            base.OnPropertyChanged(propertyName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendReorderStarted()
        {
            ReorderStartedCommand?.Execute(null);
            ReorderStarted?.Invoke(this, EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendReorderCompleted()
        {
            ReorderCompletedCommand?.Execute(null);
            ReorderCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}