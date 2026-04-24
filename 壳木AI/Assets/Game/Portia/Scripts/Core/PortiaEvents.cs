namespace Game.Portia
{
    public struct InteractTargetChangedEvent
    {
        public IInteractable Target;
    }

    public struct InventoryChangedEvent
    {
        public int Gid;
        public int NewCount;
    }

    public struct ItemReceivedEvent
    {
        public int Gid;
        public int Count;
    }
}
