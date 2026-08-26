using System.Threading;
using System.Threading.Tasks;

namespace WeGotOne.Fish_Interface
{   
    
    /// <summary>
    ///  물고기 상태의 기본 인터페이스
    ///  상태 진입/퇴장을 비동기 처리하며, CancellationToken으로
    ///  진행중인 전환이 다른전환에 의해 취소 될수 있음을 표현함
    /// </summary>
    public interface IFishState
    {
        
            // 상태에 진입할 때 1회 실행, 애니메이션/트윈 등 디봉기 연출이 있다면 여기서 await 해야함
            Task EnterAsync(FishStateManager owner, CancellationToken token);
            
            // 상태 유지 중 매 프레임 호출되는 동기로직 (필요 상태만 구현)
            void Tick();
            
            // 상태 퇴장 시 1회 실행, 정리 로직
            Task ExitAsync(CancellationToken token);
        
    }
}