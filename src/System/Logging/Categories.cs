/***************************************************************************
	Categories.cs

	Categories used for logging
***************************************************************************/

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Categories used for logging
	/// </summary>
	public static class Categories
	{
		/// <summary>
		/// Argument validation category
		/// </summary>
		public static readonly Category ArgumentValidation = new Category("ArgumentValidation");

		/// <summary>
		/// Omex Common
		/// </summary>
		public static Category Common { get; } = new Category("OmexCommon");

		/// <summary>
		/// Omex Configuration DataSet
		/// </summary>
		public static Category ConfigurationDataSet { get; } = new Category("OmexConfigurationDataSet");

		/// <summary>
		/// Omex Infrastructure
		/// </summary>
		public static Category Infrastructure { get; } = new Category("OmexInfrastructure");

		/// <summary>
		/// Omex Experimentation
		/// </summary>
		public static Category Experimentation { get; } = new Category("OmexExperimentation");

		/// <summary>
		/// Omex Gate DataSet
		/// </summary>
		public static Category GateDataSet { get; } = new Category("OmexGateDataSet");


		/// <summary>
		/// Omex Gate Selection
		/// </summary>
		public static Category GateSelection { get; } = new Category("OmexGateSelection");

		/// <summary>
		/// Omex TestGroups DataSet
		/// </summary>
		public static Category TestGroupsDataSet { get; } = new Category("OmexTestGroupsDataSet");

		/// <summary>
		/// Omex Timing General
		/// </summary>
		public static Category TimingGeneral { get; } = new Category("OmexTimingGeneral");

		/// <summary>
		/// Omex Transaction General
		/// </summary>
		public static Category TransactionGeneral { get; } = new Category("OmexTransactionGeneral");
	}
}
