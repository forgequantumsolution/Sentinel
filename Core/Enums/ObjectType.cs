namespace Core.Enums
{
    /// <summary>
    /// Types of objects on which permissions can be applied.
    /// This enum defines the different types of resources/objects in the system.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// Application feature or module (e.g., Dynamic Forms, Analytics Dashboard)
        /// </summary>
        Feature,
        
        /// <summary>
        /// API endpoint or route (e.g., /api/users, /api/reports)
        /// </summary>
        Url,
        
        /// <summary>
        /// File or document (e.g., report.pdf, data.csv)
        /// </summary>
        File,
        
        /// <summary>
        /// Database table or entity
        /// </summary>
        DatabaseTable,
        
        /// <summary>
        /// UI component or widget
        /// </summary>
        UIComponent,
        
        /// <summary>
        /// Workflow or process
        /// </summary>
        Workflow,
        
        /// <summary>
        /// Report or dashboard
        /// </summary>
        Report,
        
        /// <summary>
        /// Custom object type
        /// </summary>
        Custom
    }
}