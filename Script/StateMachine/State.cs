using System.Collections.Generic;
using System.Linq;
using MUtility;
using UnityEngine;

namespace PlayBox
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class State : MonoBehaviour
    {
        [SerializeField, HideInInspector] StateMachineType stateMachineType = StateMachineType.OneEnabled;
        [SerializeField, HideInInspector] internal List<State> innerStates = new List<State>();
        [SerializeField, HideInInspector] List<State> selectedStates = new List<State>();
        [SerializeField, HideInInspector] List<State> defaultStates = new List<State>();

        [SerializeField, HideInInspector] public Color stateColor = Color.black;
        [SerializeField, HideInInspector] State parentStateMachine;
        [SerializeField, HideInInspector] StateEffect[] effects;
        [SerializeField, HideInInspector] StateTransition[] transitions;

        public delegate void StateEnterDelegate(State previousState);
        public delegate void StateExitDelegate(State nextState);
        public delegate void InnerStateChangeDelegate(State previousState, State currentState);

        public event InnerStateChangeDelegate InnerStateChanged;
        public event StateEnterDelegate EnteredInThisState;
        public event StateExitDelegate ExitedFromThisState;

        // SETUP

#if UNITY_EDITOR
        void Update()
        {
            if (Application.isPlaying) return;
            UpdateState();
        }
#endif
        internal void UpdateState()
        {
            FindParentState();
            FindChildStates();
            FixDefaultStates();
            FixSelectedStates();
            UpdateEffects();
            UpdateTransitions();
        }

        void Awake()
        {
            FindParentState();
            FindChildStates();
            FixDefaultStates();

            SetDefaultsToSelected(false);

            UpdateEffects();
            UpdateTransitions();
            InvokeEffectsOnAwake();
        }


        /*
        internal void UpdateStateMachineTreeUp(string starter)
        {
            FindParentState();
            if (parentStateMachine != null) 
                parentStateMachine.UpdateStateMachineTreeUp(starter);
            else
            {
                Debug.Log($"Full Fresh {starter}");
                FindChildStates(); 
                foreach (State innerState in innerStates)
                    innerState.UpdateStateMachineTreeDown(this);
            }
        }

        internal void UpdateStateMachineTreeDown(State parentState)
        {
            parentStateMachine = parentState;
            FindChildStates();
            FixDefaultStates();
            FixSelectedStates();
            UpdateEffects();
            UpdateTransitions();

            foreach (State state in innerStates)
                state.UpdateStateMachineTreeDown(this);
        }
        */

        internal void FindParentState()
        {
            Transform parent = transform.parent;
            if (parent == null || !parent.gameObject.activeInHierarchy)
            {
                parentStateMachine = null;
                return;
            } 

            parentStateMachine = parent.GetComponent<State>();
        }

        internal void FindChildStates()
        {
            innerStates.Clear();
            for (var i = 0; i < transform.childCount; i++)
            {
                if (!transform.gameObject.activeInHierarchy) continue;
                if (transform.GetChild(i).TryGetComponent(out State state))
                    innerStates.Add(state);
            }

            FixDefaultStates();
            FixSelectedStates();
        }

        void UpdateEffects() => effects = GetComponents<StateEffect>();

        void UpdateTransitions() => transitions = GetComponents<StateTransition>();


        // PUBLIC GETTERS & SETTERS

        public IReadOnlyList<State> InnerStates => innerStates;

        public IEnumerable<State> SelectableInnerStates
        {
            get
            {
                if (innerStates == null) yield break;
                foreach (State state in innerStates)
                    if (state.IsSelectableState)
                        yield return state;
            }
        }

        public State ParentStateMachine
        {
            get => parentStateMachine;
            internal set => parentStateMachine = value;
        }

        public IReadOnlyList<State> SelectedInnerStates
        {
            get
            {
                if (stateMachineType == StateMachineType.OneEnabled && selectedStates.IsEmpty())
                    SetDefaultsToSelected(invokeEvents: false);
                return selectedStates;
            }
        }

        public bool IsDefaultSate => ParentStateMachine == null || ParentStateMachine.defaultStates.Contains(this);
        public bool IsSelectedState => ParentStateMachine == null || ParentStateMachine.SelectedInnerStates.Contains(this);

        public bool IsSelectableState => ParentStateMachine == null || (ParentStateMachine.IsSelectedState && ParentStateMachine.IsSelectableState);

        public bool HasInnerStates => !innerStates.IsNullOrEmpty();

        public void SelectState()
        {
            if (parentStateMachine == null) return;
            parentStateMachine.TryAddSelectedState(this);
        }

        public void DeselectState()
        {
            if (parentStateMachine == null) return;
            parentStateMachine.TryRemoveSelectState(this);
        }

        public bool TryAddSelectedState(State state)
        {
            if (state == null) return false;
            if (!innerStates.Contains(state)) return false;
            if (selectedStates.Contains(state)) return false;
            State oldState = null;
            if (stateMachineType != StateMachineType.MultipleEnabled)
                while (selectedStates.Count > 0)
                {
                    oldState = selectedStates[0];
                    selectedStates.Remove(oldState);
                    oldState.InvokeExit(state);
                }

            selectedStates.Add(state);
            state.InvokeExit(oldState);
            InnerStateChanged?.Invoke(oldState, state);
            return true;
        }

        public bool TryRemoveSelectState(State state)
        {
            if (state == null) return false;
            if (!innerStates.Contains(state)) return false;
            if (!selectedStates.Contains(state)) return false;
            selectedStates.Remove(state);
            state.InvokeExit(nextState: null);
            InnerStateChanged?.Invoke(state, currentState: null);
            if (stateMachineType == StateMachineType.OneEnabled && selectedStates.IsEmpty())
                SetDefaultsToSelected(invokeEvents: true);
            return true;
        }

        public bool TryChangeSelectedState(State oldState, State newState)
        {
            if (newState == this) return false;
            if (!selectedStates.Contains(oldState)) return false;
            if (selectedStates.Contains(newState)) return false;

            selectedStates.Remove(oldState);
            oldState.InvokeExit(newState);
            selectedStates.Add(newState);
            InnerStateChanged?.Invoke(oldState, newState);
            newState.InvokeEnter(oldState);
            return true;
        }

        public void SetAsDefault()
        {
            if (parentStateMachine == null) return;
            parentStateMachine.TryAddDefault(this);
        }

        public void UnsetAsDefault()
        {
            if (parentStateMachine == null) return;
            parentStateMachine.TryRemoveDefault(this);
        }

        public bool TryAddDefault(State state)
        {
            if (state == null) return false;
            if (!innerStates.Contains(state)) return false;
            if (defaultStates.Contains(state)) return false;
            if (stateMachineType != StateMachineType.MultipleEnabled)
                defaultStates.Clear();

            defaultStates.Add(state);
            return true;
        }


        public bool TryRemoveDefault(State state)
        {
            if (state == null) return false;
            if (!innerStates.Contains(state)) return false;
            if (!defaultStates.Contains(state)) return false;
            if (stateMachineType == StateMachineType.OneEnabled && defaultStates.Count <= 1) return false;
            defaultStates.Remove(state);
            return true;
        }


        public StateMachineType StateMachineType
        {
            get => stateMachineType;
            set
            {
                if (Application.isPlaying) return;
                if (value == stateMachineType) return;
                stateMachineType = value;

                FixDefaultStates();

                FixSelectedStates();
            }
        }

        // PRIVATE

        void FixDefaultStates()
        {
            for (int i = defaultStates.Count - 1; i >= 0; i--)
                if (defaultStates[i] == null || !innerStates.Contains(defaultStates[i]))
                    defaultStates.RemoveAt(i);

            if (stateMachineType != StateMachineType.MultipleEnabled)
                while (defaultStates.Count > 1)
                    defaultStates.RemoveAt(defaultStates.Count - 1);

            if (stateMachineType == StateMachineType.OneEnabled)
                if (defaultStates.IsEmpty() && innerStates.Count > 0)
                    defaultStates.Add(innerStates[0]);
        }

        void FixSelectedStates()
        {
            for (int i = selectedStates.Count - 1; i >= 0; i--)
                if (selectedStates[i] == null || !innerStates.Contains(selectedStates[i]))
                    selectedStates.RemoveAt(i);

            if (stateMachineType != StateMachineType.MultipleEnabled)
                while (selectedStates.Count > 1)
                    selectedStates.RemoveAt(selectedStates.Count - 1);

            if (stateMachineType == StateMachineType.OneEnabled)
                if (selectedStates.IsEmpty() && selectedStates.Count > 0)
                    SetDefaultsToSelected(false);
        }


        void SetDefaultsToSelected(bool invokeEvents)
        {
            selectedStates.Clear();
            foreach (State innerState in defaultStates)
            {
                selectedStates.Add(innerState);
                if (invokeEvents)
                {
                    innerState.InvokeEnter(previousState: null);
                    InnerStateChanged?.Invoke(previousState: null, innerState);
                }
            }
        }

        void InvokeEffectsOnAwake()
        {
            foreach (State state in InnerStates)
                if (!state.IsSelectedState)
                    state.InvokeEffectsOnAwake(selected: false);
            foreach (State state in InnerStates)
                if (state.IsSelectedState)
                    state.InvokeEffectsOnAwake(selected: false);
        }

        IEnumerable<StateEffect> Effects
        {
            get
            {
                GameObject go;
                try
                {
                    go = gameObject;
                }
                catch (MissingReferenceException)
                {
                    yield break;
                }

                if (!Application.isPlaying && go != null)
                    UpdateEffects();
                foreach (StateEffect effect in effects)
                    yield return effect;
            }
        }

        IEnumerable<StateTransition> Transition
        {
            get
            {
                GameObject go;
                try
                {
                    go = gameObject;
                }
                catch (MissingReferenceException)
                {
                    yield break;
                }

                if (!Application.isPlaying && go != null)
                    UpdateTransitions();
                foreach (StateTransition transition in transitions)
                    yield return transition;
            }
        }
        void InvokeEnter(State previousState)
        {
            EnteredInThisState?.Invoke(previousState);
            foreach (StateEffect effect in Effects)
                effect.OnStateEnter(this);
        }

        void InvokeExit(State nextState)
        {
            ExitedFromThisState?.Invoke(nextState);
            foreach (StateEffect effect in Effects)
                effect.OnStateExit(this);
        }

        internal void InvokeEffectsOnAwake(bool selected)
        {
            UpdateEffects();
            foreach (StateEffect effect in Effects)
                effect.InvokeEffectOnAwake(selected, this);
        }
    }
}