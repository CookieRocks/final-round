using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class DeskPrototypeController : MonoBehaviour
{
    public enum DeskPrototypeState
    {
        Standby,
        LaptopFocus,
        JobListing,
        ApplicationChoice,
        Recruiter,
        Prep,
        TransitioningToRoom
    }

    [SerializeField] private string interviewRoomSceneName = "InterviewRoom";
    [SerializeField] private DeskPrototypeState currentState = DeskPrototypeState.Standby;
    [SerializeField] private JobListingData defaultJobListing;

    public DeskPrototypeState CurrentState => currentState;

    public void BeginDeskRun()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        if (defaultJobListing != null)
        {
            state.SelectedJobId = defaultJobListing.jobId;
        }

        currentState = DeskPrototypeState.LaptopFocus;
        Debug.Log("Final Round P26: Desk run started.\n" + state.BuildDebugSummary());
    }

    public CandidateState CreateNeutralCandidateState()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        currentState = DeskPrototypeState.Standby;
        return state;
    }

    public void GoToInterviewRoom()
    {
        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        currentState = DeskPrototypeState.TransitioningToRoom;
        Debug.Log(
            $"Final Round P26: Desk-to-Room transition requested. Scene: {interviewRoomSceneName}\n" +
            state.BuildDebugSummary());

        SceneManager.LoadScene(interviewRoomSceneName);
    }

    public void ResetDeskRun()
    {
        if (FinalRoundRunState.HasInstance)
        {
            FinalRoundRunState.Instance.ResetRun();
        }

        currentState = DeskPrototypeState.Standby;
        Debug.Log("Final Round P26: Desk run reset.");
    }
}
