namespace PrototypeSneaking.Domain.Stage
{
    public interface IStraightTrackingHabitStateMachine
    {
        bool IsWondering { get; }
        bool IsTracking { get; }
        bool IsReachedAttackDistance { get; }
        bool IsJustLost { get; }
        bool IsGoingBack { get; }
        void ToWonder();
        void ToTrack();
        void ToReachAttackDistance();
        void JustToLose();
        void ToGoBack();
        // デバッグ用
        void Log();
    }

    public class StraightTrackingHabitStateMachine : IStraightTrackingHabitStateMachine
    {
        private State state;
        // TODO: デバッグ用
        public void Log() => UnityEngine.Debug.Log($"state: {state}");
        private enum State
        {
            Wondering,
            Tracking,
            ReachedAttackDistance,
            JustLost,
            GoBack
        }

        public bool IsWondering => state == State.Wondering;
        public bool IsTracking => state == State.Tracking;
        public bool IsReachedAttackDistance => state == State.ReachedAttackDistance;
        public bool IsJustLost => state == State.JustLost;
        public bool IsGoingBack => state == State.GoBack;

        public StraightTrackingHabitStateMachine()
        {
            state = State.Wondering;
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

        public void JustToLose()
        {
            Allow(allowedStates: new State[] { State.Tracking, State.ReachedAttackDistance }, toState: State.JustLost);
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
                AllowOnly(allowedState, toState);
            }
        }

        private void AllowOnly(State allowedState, State toState)
        {
            if (state == allowedState) { return; }
            throw new StraightTrackingHabitStateMachineException($"{state} からいきなり {toState} には変更できません");
        }
    }

    public class StraightTrackingHabitStateMachineException : System.Exception
    {
        public StraightTrackingHabitStateMachineException(string message) : base(message) { }
    }
}