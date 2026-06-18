using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class FighterAgent : Agent 
{
    [Header("The Fighters")]
    public NewFighter myBody;      // The fighter this AI controls
    public NewFighter opponent;    // The enemy we need to beat up

    // Track health to know when someone gets hit
    private int myLastHealth;
    private int opponentLastHealth;

    // 1. INITIALIZE: Called once when the game starts
    public override void Initialize()
    {
        myLastHealth = myBody.maxHealth;
        opponentLastHealth = opponent.maxHealth;
    }

    // 2. EPISODE BEGIN: Reset the arena for the next round
    public override void OnEpisodeBegin()
    {
        // Reset both fighters to full health and starting positions
        bool startOnLeft = myBody.IsOnLeftSide; 
        myBody.ResetFighter(startOnLeft);

        myLastHealth = myBody.maxHealth;
        opponentLastHealth = opponent.maxHealth;
    }

    // 3. THE EYES: This is where we feed the Neural Network numbers
    public override void CollectObservations(VectorSensor sensor)
    {
        // Safety check in case the fighters aren't assigned
        if (myBody == null || opponent == null) return;

        // --- MY AWARENESS (5 Observations) ---
        sensor.AddObservation(myBody.currentHealth / (float)myBody.maxHealth);
        sensor.AddObservation(myBody.transform.position.x);
        sensor.AddObservation(myBody.transform.position.y);
        sensor.AddObservation(myBody.velocity.x);
        sensor.AddObservation(myBody.velocity.y);

        // --- OPPONENT AWARENESS (5 Observations) ---
        sensor.AddObservation(opponent.currentHealth / (float)opponent.maxHealth);
        sensor.AddObservation(opponent.transform.position.x);
        sensor.AddObservation(opponent.transform.position.y);
        sensor.AddObservation(opponent.velocity.x);
        sensor.AddObservation(opponent.velocity.y);

        // --- ARENA AWARENESS (1 Observation) ---
        sensor.AddObservation(Vector3.Distance(myBody.transform.position, opponent.transform.position));
    }

    // 4. THE MUSCLES: The Neural Network spits out numbers here.
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Safety check to make sure the body is ready
        if (myBody == null || myBody.currentState == null) return;

        // Grab the AI's choices from the neural network branches
        int moveAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1];

        // --- 1. MOVEMENT SYSTEM ---
        int numpadDirection = 5; // Default to Idle
        bool isJumpPressed = false;

        switch (moveAction)
        {
            case 0: numpadDirection = 5; break; // Idle
            case 1: numpadDirection = 6; break; // Walk Forward
            case 2: numpadDirection = 4; break; // Walk Backward
            case 3: numpadDirection = 8; isJumpPressed = true; break; // Jump
        }

        // Forge the fake input packet and inject it for movement
        InputData fakeInput = new InputData();
        fakeInput.direction = numpadDirection;
        fakeInput.jumpPressed = isJumpPressed;
        myBody.currentState.Update(fakeInput);

        // --- 2. ATTACK SYSTEM (The Smart Way) ---
        if (attackAction == 1 && myBody.actions.Count > 0)
        {
            List<ActionData> triggerAttack = new List<ActionData>();
            triggerAttack.Add(myBody.actions[0]);
            myBody.SwitchAction(triggerAttack);
        }
    }

    // 5. THE TEACHER: Checking for rewards and punishments every frame
    void FixedUpdate()
    {
        if (myBody == null || opponent == null) return;

        // THE STICK: Did I lose health?
        if (myBody.currentHealth < myLastHealth)
        {
            AddReward(-0.1f); // Small penalty for getting hit
            myLastHealth = myBody.currentHealth;
        }

        // THE CARROT: Did the opponent lose health?
        if (opponent.currentHealth < opponentLastHealth)
        {
            AddReward(0.1f); // Small reward for landing a hit!
            opponentLastHealth = opponent.currentHealth;
        }

        // END CONDITIONS: Did somebody die?
        if (myBody.currentHealth <= 0)
        {
            AddReward(-1.0f); // Massive penalty for dying
            EndEpisode();     // Stop the simulation
        }
        else if (opponent.currentHealth <= 0)
        {
            AddReward(1.0f); // Massive reward for a Knockout!
            EndEpisode();    // Stop the simulation
        }
    }
}