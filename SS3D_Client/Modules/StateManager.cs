﻿using System;
using System.Reflection;

using Mogre;
using Miyagi;

using Lidgren.Network;
using SS3D.Modules.Network;

namespace SS3D.Modules
{
  /************************************************************************/
  /* state manager for program states                                     */
  /************************************************************************/
  public class StateManager
  {

    private OgreManager mEngine;
    public State mCurrentState
    {
        private set;
        get;
    }
    private Type mNewState;

    public OgreManager Engine
    {
      get { return mEngine; }
    }

    #region Startup , Shutdown , Constructor
    public StateManager(OgreManager _engine)
    {
        // constructor
        mEngine = _engine;
        mCurrentState = null;
        mNewState = null;
    }

    public bool Startup(Type _firstState)
    {
        // start up and initialize the first state        
        // can't start up the state manager again if it's already running
        if (mCurrentState != null || mNewState != null)
            return false;

        // initialize with first state
        if (!RequestStateChange(_firstState))
            return false;

        // OK
        return true;
    }

    public void Shutdown()
    {
        // if a state is active, shut down the state to clean up
        if (mCurrentState != null)
            SwitchToNewState(null);

        // make sure any pending state change request is reset
        mNewState = null;
    } 
    #endregion

    #region Input

    public void UpdateInput(FrameEvent evt, MOIS.Keyboard keyState, MOIS.Mouse mouseState)
    {
        // if a state is active, call the states input update.
        if (mCurrentState != null)
            mCurrentState.UpdateInput(evt, keyState, mouseState);
    }

    public void KeyDown(MOIS.KeyEvent keyState)
    {
        // if a state is active, call the states keydown method.
        if (mCurrentState != null)
            mCurrentState.KeyDown(keyState);
    }

    public void KeyUp(MOIS.KeyEvent keyState)
    {
        // if a state is active, call the states keyup method.
        if (mCurrentState != null)
            mCurrentState.KeyUp(keyState);
    }

    public void MouseUp(MOIS.MouseEvent mouseState, MOIS.MouseButtonID button)
    {
        // if a state is active, call the states mouseup method.
        if (mCurrentState != null)
            mCurrentState.MouseUp(mouseState, button);
    }

    public void MouseDown(MOIS.MouseEvent mouseState, MOIS.MouseButtonID button)
    {
        // if a state is active, call the states mousedown method.
        if (mCurrentState != null)
            mCurrentState.MouseDown(mouseState, button);
    }

    public void MouseMove(MOIS.MouseEvent mouseState)
    {
        // if a state is active, call the states mousemove method.
        if (mCurrentState != null)
            mCurrentState.MouseMove(mouseState);
    }

    #endregion

    #region Updates & Statechanges
    public void Update(long _frameTime)
    {
        // check if a state change was requested
        if (mNewState != null)
        {
            State newState = null;

            // use reflection to get new state class default constructor
            ConstructorInfo constructor = mNewState.GetConstructor(Type.EmptyTypes);

            // try to create an object from the requested state class
            if (constructor != null)
                newState = (State)constructor.Invoke(null);

            // remove all current ui elements

            // switch to the new state if an object of the requested state class could be created
            if (newState != null)
                SwitchToNewState(newState);

            // reset state change request until next state change is requested
            mNewState = null;
        }

        // if a state is active, update the active state
        if (mCurrentState != null)
        {
            mCurrentState.Update(_frameTime);
        }
    }

    public bool RequestStateChange(Type _newState)
    {
        // set next state that should be switched to, returns false if invalid

        // new state class must be derived from base class "State"
        if (_newState == null || !_newState.IsSubclassOf(typeof(State)))
            return false;

        // don't change the state if the requested state class matches the current state
        if (mCurrentState != null && mCurrentState.GetType() == _newState)
            return false;

        // store type of new state class to request a state change
        mNewState = _newState;

        // OK
        return true;
    }

    //////////////////////////////////////////////////////////////////////////
    // internal functions ////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////

    private void SwitchToNewState(State _newState)
    {
        //change from one state to another state 
        //if a state is active, shut it down
        if (mCurrentState != null)
            mCurrentState.Shutdown();

        // switch to the new state, might be null if no new state should be activated
        mCurrentState = _newState;

        // if a state is active, start it up
        if (mCurrentState != null)
            mCurrentState.Startup(this);
    } 
    #endregion

  }

}
