namespace WeGotOne.Fish_Interface
{   
    
    /// <summary>
    /// HandController에 의해 잡히고 놓일 수 있는 오브젝트가 구현해야 하는 인터페이스
    /// HandController는 인터페이스만 알면되고, Fish의 구체적인 상태 머신 구조는 몰라도 됨
    /// </summary>
    public interface ICatchable
    {
        void OnGrabbed(HandControllerV2 hand);
        void OnReleased(HandControllerV2 hand);
    }
}