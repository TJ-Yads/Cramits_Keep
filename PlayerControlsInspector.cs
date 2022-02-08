using UnityEditor;
//this script affects the unity editor to allow for ease of use
//puts each major part of player controls scripting intro seperate sections that can be seen by a dropdown menu

    //tell unity what script is having its editor changed
[CustomEditor(typeof(PlayerControls))]
public class PlayerControlsInspector : Editor
{
    //create enum with various main categories
    public enum DisplayCategory
    {
        BasicVariables, HiddenData, UIData, StatsList, TestData, PowerData
    }
    //enum field to decide what is show
    public DisplayCategory categoryToDisplay;
    //function to run the editor
    public override void OnInspectorGUI()
    {
        //display enum popup
        categoryToDisplay = (DisplayCategory)EditorGUILayout.EnumPopup("Display", categoryToDisplay);

        //create space for popup
        EditorGUILayout.Space();

        //switch statment to manage the swapping of categories
        switch (categoryToDisplay)
        {
            case DisplayCategory.BasicVariables:
                DisplayBasicVariables();
                break;

            case DisplayCategory.HiddenData:
                DisplayHiddenData();
                break;

            case DisplayCategory.UIData:
                DisplayUIData();
                break;

            case DisplayCategory.StatsList:
                DisplayStatsData();
                break;

            case DisplayCategory.TestData:
                DisplayTestData();
                break;

            case DisplayCategory.PowerData:
                DisplayPowerData();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
    //each function will display the diffrent groups of info
    void DisplayHiddenData()//hidden data is variables that need public access but does not need to be changed in in inspector
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("upgradeUIScript"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityManagerScript"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemManagerScript"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CanAbility"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("designatedIntValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionInUI"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Bullet"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("melee"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeMainOBJ"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("newSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityNumCount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityNumIsCooling"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mousePoint"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentCurrency"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeCenter"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("handGlow"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("castingAudio"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("walkingSource"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RayEndPoint"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerBack"));
    }
    void DisplayUIData()//any important UI related element 
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("primaryCooldownVisual"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fireballCurrentCounter"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityTitleText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityEffectText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldownTextOBJs"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AbilityIMGs"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityCooldownIMGs"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("chargedAbilityOBJ"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UIPoints"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerCurrencyCounter"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sectionsArray"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityCounterText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leechText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leftSideUI"));
    }
    void DisplayBasicVariables()//base values of generic player data
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseDMG"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fireballMax"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fireballCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FireRatePrim"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("castTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeedReduction"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fireRateSecond"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletSpawn"));
    }
    void DisplayStatsData()//stat data that will only show its info if the stat has a least 1 in it
    {
        //this int property check is used to hide the data if the value is 0 or lower
        SerializedProperty hasSTR = serializedObject.FindProperty("strength");
        EditorGUILayout.PropertyField(hasSTR);

        if (hasSTR.floatValue > 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("knockbackResist"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeVamp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityDMGBonus"));
        }
        EditorGUILayout.Space();

        SerializedProperty hasINT = serializedObject.FindProperty("intellect");
        EditorGUILayout.PropertyField(hasINT);

        if (hasINT.floatValue > 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spellDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fireballCapacity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldownReduction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityDMGBonus"));
        }
        EditorGUILayout.Space();

        SerializedProperty hasAGL = serializedObject.FindProperty("agility");
        EditorGUILayout.PropertyField(hasAGL);

        if (hasAGL.floatValue > 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debuffBonusDuration"));
        }
        EditorGUILayout.Space();

        SerializedProperty hasVIT = serializedObject.FindProperty("vitality");
        EditorGUILayout.PropertyField(hasVIT);

        if (hasVIT.floatValue > 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debuffReduction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debuffBonusDuration"));
        }
    }
    void DisplayTestData()//any variable that was in a testing phase was placed here
    {

    }
    void DisplayPowerData()//data showing the players collection of powers
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activePrimaryEffects"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activeMeleeEffects"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activeAbilityEffects"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bombSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bombTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("splitRoundTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("incendiaryMagicTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bleedClawTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityHealTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeBoosterTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeBoosterDMGScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("oilClawTotal"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clarityTotal"));
    }
}
