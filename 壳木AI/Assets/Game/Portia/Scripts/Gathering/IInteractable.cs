namespace Game.Portia
{
    public interface IInteractable
    {
        string PromptText { get; }
        void Interact(UnityEngine.GameObject initiator);
    }
}
