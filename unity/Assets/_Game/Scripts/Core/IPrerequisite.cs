namespace PCTechnician.Core
{
    /// <summary>
    /// Contract for prerequisite validation rules before an action can be performed.
    /// </summary>
    public interface IPrerequisite
    {
        string PrerequisiteDescription { get; }
        bool IsSatisfied { get; }
        string GetUnsatisfiedMessage();
    }
}
