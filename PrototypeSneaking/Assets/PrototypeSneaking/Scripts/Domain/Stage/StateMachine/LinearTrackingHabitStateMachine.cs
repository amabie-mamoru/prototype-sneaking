namespace PrototypeSneaking.Domain.Stage
{
    public interface ILinearTrackingHabitStateMachine
    {
        bool IsWondering { get; }
        bool IsTracking { get; }
        bool IsReachedAttackDistance { get; }
        bool IsSearchingAttackObj { get; }
        bool IsLostAttackObj { get; }
        bool IsAttacking { get; }
        bool IsJustLost { get; }
        bool IsGoingBack { get; }
        void ToDisable();
        void ToWonder();
        void ToTrack();
        void ToReachAttackDistance();
        void ToSearchAttackObj();
        void ToLoseAttackObj();
        void ToAttack();
        void JustToLose();
        void ToGoBack();
#if UNITY_EDITOR
        // デバッグ用
        void DebugLog(string name);
#endif
    }

    public class LinearTrackingHabitStateMachine : ILinearTrackingHabitStateMachine
    {
        private State state;
        private enum State
        {
            None,
            Wondering,
            Tracking,
            ReachedAttackDistance,
            SearchingAttackObj,
            LostAttackObj,
            Attacking,
            JustLost,
            GoBack
        }

        public bool IsWondering => state == State.Wondering;
        public bool IsTracking => state == State.Tracking;
        public bool IsReachedAttackDistance => state == State.ReachedAttackDistance;
        public bool IsSearchingAttackObj => state == State.SearchingAttackObj;
        public bool IsLostAttackObj => state == State.LostAttackObj;
        public bool IsAttacking => state == State.Attacking;
        public bool IsJustLost => state == State.JustLost;
        public bool IsGoingBack => state == State.GoBack;

        public LinearTrackingHabitStateMachine()
        {
            state = State.Wondering;
        }

        public void ToDisable()
        {
            state = State.None;
        }

        public void ToWonder()
        {
            AllowOnly(allowedState: State.GoBack, toState: State.Wondering);
            state = State.Wondering;
        }

        public void ToTrack()
        {
            AllowOnly(allowedState: State.Wondering, toState: State.Tracking);
            state = State.Tracking;
        }

        public void ToReachAttackDistance()
        {
            AllowOnly(allowedState: State.Tracking, toState: State.ReachedAttackDistance);
            state = State.ReachedAttackDistance;
        }

        public void ToSearchAttackObj()
        {
            AllowOnly(allowedState: State.ReachedAttackDistance, toState: State.SearchingAttackObj);
            state = State.SearchingAttackObj;
        }

        public void ToLoseAttackObj()
        {
            AllowOnly(allowedState: State.SearchingAttackObj, toState: State.LostAttackObj);
            state = State.LostAttackObj;
        }

        public void ToAttack()
        {
            AllowOnly(allowedState: State.ReachedAttackDistance, toState: State.Attacking);
            state = State.Attacking;
        }

        public void JustToLose()
        {
            Allow(allowedStates: new State[] { State.Tracking, State.LostAttackObj }, toState: State.JustLost);
            state = State.JustLost;
        }

        public void ToGoBack()
        {
            AllowOnly(allowedState: State.JustLost, toState: State.GoBack);
            state = State.GoBack;
        }

        private void Allow(State[] allowedStates, State toState)
        {
            foreach (var allowedState in allowedStates)
            {
                if (state == allowedState) { return; }
            }
            throw new LinearTrackingHabitStateMachineException($"{state} からいきなり {toState} には変更できません");
        }

        private void AllowOnly(State allowedState, State toState)
        {
            if (state == allowedState) { return; }
            throw new LinearTrackingHabitStateMachineException($"{state} からいきなり {toState} には変更できません");
        }

#if UNITY_EDITOR
        private State prevState;
        private bool isInit = true;
        // TODO: デバッグ用
        public void DebugLog(string name)
        {
            if (isInit)
            {
                prevState = state;
                UnityEngine.Debug.Log($"{name} init state: {state}");
                isInit = false;
                return;
            }
            if (prevState == state) { return; }
            UnityEngine.Debug.Log($"{name} changed state: {prevState} to {state}");
            prevState = state;
        }
#endif
    }

    public class LinearTrackingHabitStateMachineException : System.Exception
    {
        public LinearTrackingHabitStateMachineException(string message) : base(message) { }
    }
}