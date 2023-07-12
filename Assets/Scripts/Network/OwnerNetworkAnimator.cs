using Unity.Netcode.Components;

//overwritten to be able to give control to client as well, for the sake of the host-client architecture
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}