using UnityEngine;

public sealed class FinalRoundRunState : MonoBehaviour
{
    private static FinalRoundRunState instance;

    [SerializeField] private CandidateState candidateState = CandidateState.CreateNeutral(false);

    public static FinalRoundRunState Instance => instance;
    public static bool HasInstance => instance != null;
    public CandidateState State => candidateState;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Final Round RunState: duplicate FinalRoundRunState destroyed.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (candidateState == null)
        {
            candidateState = CandidateState.CreateNeutral(false);
        }
    }

    public static FinalRoundRunState EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject runStateObject = new GameObject("Final Round Run State");
        return runStateObject.AddComponent<FinalRoundRunState>();
    }

    public static CandidateState CreateNeutralRun()
    {
        FinalRoundRunState runState = EnsureInstance();
        runState.candidateState = CandidateState.CreateNeutral(true);
        Debug.Log("Final Round RunState: CandidateState created.\n" + runState.candidateState.BuildDebugSummary());
        return runState.candidateState;
    }

    public static bool HasActiveRun()
    {
        return instance != null
            && instance.candidateState != null
            && instance.candidateState.HasActiveDeskRun;
    }

    public static bool TryGetActiveState(out CandidateState state)
    {
        if (HasActiveRun())
        {
            state = instance.candidateState;
            return true;
        }

        state = null;
        return false;
    }

    public void ResetRun()
    {
        candidateState = CandidateState.CreateNeutral(false);
        Debug.Log("Final Round RunState: CandidateState reset.\n" + candidateState.BuildDebugSummary());
    }
}
