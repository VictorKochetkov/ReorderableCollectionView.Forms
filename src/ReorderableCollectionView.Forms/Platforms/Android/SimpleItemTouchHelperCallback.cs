using System;
using AndroidX.RecyclerView.Widget;
using Xamarin.Forms.Platform.Android;

namespace ReorderableCollectionView.Forms
{
    public class SimpleItemTouchHelperCallback : ItemTouchHelper.Callback
    {
        Action _movementStarted;
        Action _movementFinished;
        IItemTouchHelperAdapter _adapter;

        public override bool IsLongPressDragEnabled => true;

        public SimpleItemTouchHelperCallback(Action movementStarted, Action movementFinished)
        {
            _movementStarted = movementStarted;
            _movementFinished = movementFinished;
        }

        public override void OnSelectedChanged(RecyclerView.ViewHolder viewHolder, int actionState)
        {
            base.OnSelectedChanged(viewHolder, actionState);

            if (actionState == ItemTouchHelper.ActionStateDrag)
            {
                _movementStarted?.Invoke();
            }
            else if (actionState == ItemTouchHelper.ActionStateIdle)
            {
                _movementFinished?.Invoke();
            }
        }

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            var itemViewType = viewHolder.ItemViewType;
            if (itemViewType == ItemViewType.Header || itemViewType == ItemViewType.Footer
                || itemViewType == ItemViewType.GroupHeader || itemViewType == ItemViewType.GroupFooter)
            {
                return MakeMovementFlags(0, 0);
            }

            var dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down;
            return MakeMovementFlags(dragFlags, 0);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            if (viewHolder.ItemViewType != target.ItemViewType)
            {
                return false;
            }

            return _adapter.OnItemMove(viewHolder.BindingAdapterPosition, target.BindingAdapterPosition);
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
        }

        public void SetAdapter(IItemTouchHelperAdapter adapter)
        {
            _adapter = adapter;
        }
    }
}