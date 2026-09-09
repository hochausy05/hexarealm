namespace HexaRealm.Interaction
{
    public interface IInteractable
    {
        bool IsInteractionAvailable(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }
}
