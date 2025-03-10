﻿using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace ReorderableCollectionView.Forms
{
    public class ReorderableCollectionViewController<TItemsView> : GroupableItemsViewController<TItemsView>
        where TItemsView : ReorderableCollectionView
    {
        nfloat _dragOffsetX = 0;
        bool _disposed;
        UILongPressGestureRecognizer _longPressGestureRecognizer;

        public ReorderableCollectionViewController(TItemsView groupableItemsView, ItemsViewLayout layout)
            : base(groupableItemsView, layout)
        {
            // The UICollectionViewController has built-in recognizer for reorder that can be installed by setting "InstallsStandardGestureForInteractiveMovement".
            // For some reason it only seemed to work when the CollectionView was inside the Flyout section of a FlyoutPage.
            // The UILongPressGestureRecognizer is simple enough to set up so let's just add our own.
            InstallsStandardGestureForInteractiveMovement = false;
        }

        public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return ItemsView?.CanReorderItems == true;
        }

        protected override UICollectionViewDelegateFlowLayout CreateDelegator()
        {
            return new ReorderableCollectionViewDelegator<TItemsView, ReorderableCollectionViewController<TItemsView>>(ItemsViewLayout, this);
        }

        protected override IItemsViewSource CreateItemsViewSource()
        {
            if (ItemsView.IsGrouped)
            {
                return ItemsSourceFactory.CreateGrouped(ItemsView.ItemsSource, this);
            }

            return ItemsSourceFactory.Create(ItemsView.ItemsSource, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_longPressGestureRecognizer != null)
                {
                    CollectionView.RemoveGestureRecognizer(_longPressGestureRecognizer);
                    _longPressGestureRecognizer.Dispose();
                    _longPressGestureRecognizer = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        void HandleLongPress(UILongPressGestureRecognizer gestureRecognizer)
        {
            var collectionView = CollectionView;
            if (collectionView == null)
                return;


            var location = gestureRecognizer.LocationInView(collectionView);

            // We are updating "CancelsTouchesInView" so views can still receive touch events when this gesture runs.
            // Those events shouldn't be aborted until they've actually moved the position of the CollectionView item.
            switch (gestureRecognizer.State)
            {
                case UIGestureRecognizerState.Began:
                    var indexPath = collectionView?.IndexPathForItemAtPoint(location);
                    if (indexPath == null)
                    {

                        return;
                    }
                    gestureRecognizer.CancelsTouchesInView = false;
                    collectionView.BeginInteractiveMovementForItem(indexPath);

                    // Drag anchor always should in the middle of cell
                    // So we need to fix it with offset
                    _dragOffsetX = (collectionView.CellForItem(indexPath).Frame.Width / 2);

                    ItemsView?.SendReorderStarted();

                    break;
                case UIGestureRecognizerState.Changed:
                    gestureRecognizer.CancelsTouchesInView = true;

                    location.X = _dragOffsetX;
                    collectionView.UpdateInteractiveMovement(location);

                    break;
                case UIGestureRecognizerState.Ended:
                    collectionView.EndInteractiveMovement();
                    break;
                default:
                    collectionView.CancelInteractiveMovement();
                    break;
            }
        }

        public override void MoveItem(UICollectionView collectionView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            var itemsSource = ItemsSource;
            var itemsView = ItemsView;

            if (itemsSource == null || itemsView == null)
            {
                return;
            }

            if (itemsSource is IGroupedItemsViewSource groupedSource)
            {
                var fromList = itemsSource.Group(sourceIndexPath) as IList;
                var fromItemsSource = fromList is INotifyCollectionChanged ? groupedSource.GroupItemsViewSource(sourceIndexPath) : null;
                var fromItemIndex = sourceIndexPath.Row;

                var toList = itemsSource.Group(destinationIndexPath) as IList;
                var toItemsSource = toList is INotifyCollectionChanged ? groupedSource.GroupItemsViewSource(destinationIndexPath) : null;
                var toItemIndex = destinationIndexPath.Row;

                if (fromList != null && toList != null)
                {
                    var fromItem = fromList[fromItemIndex];
                    SetObserveChanges(fromItemsSource, false);
                    SetObserveChanges(toItemsSource, false);
                    fromList.RemoveAt(fromItemIndex);
                    toList.Insert(toItemIndex, fromItem);
                    SetObserveChanges(fromItemsSource, true);
                    SetObserveChanges(toItemsSource, true);
                    itemsView.SendReorderCompleted();
                }
            }
            else if (itemsView.ItemsSource is IList list)
            {
                var fromPosition = sourceIndexPath.Row;
                var toPosition = destinationIndexPath.Row;
                var fromItem = list[fromPosition];
                SetObserveChanges(itemsSource, false);
                list.RemoveAt(fromPosition);
                list.Insert(toPosition, fromItem);
                SetObserveChanges(itemsSource, true);
                itemsView.SendReorderCompleted();
            }
        }

        void SetObserveChanges(IItemsViewSource itemsSource, bool enable)
        {
            if (itemsSource is IObservableItemsViewSource observableSource)
            {
                observableSource.ObserveChanges = enable;
            }
        }

        public void UpdateCanReorderItems()
        {
            if (ItemsView.CanReorderItems)
            {
                if (_longPressGestureRecognizer == null)
                {
                    _longPressGestureRecognizer = new UILongPressGestureRecognizer(HandleLongPress);
                    CollectionView.AddGestureRecognizer(_longPressGestureRecognizer);
                }
            }
            else
            {
                if (_longPressGestureRecognizer != null)
                {
                    CollectionView.RemoveGestureRecognizer(_longPressGestureRecognizer);
                    _longPressGestureRecognizer.Dispose();
                    _longPressGestureRecognizer = null;
                }
            }
        }
    }
}