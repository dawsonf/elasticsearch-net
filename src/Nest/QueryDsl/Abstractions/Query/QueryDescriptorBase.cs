﻿namespace Nest
{
	public abstract class QueryDescriptorBase<TDescriptor, TInterface>
		: DescriptorBase<TDescriptor, TInterface>, IQuery
		where TDescriptor : QueryDescriptorBase<TDescriptor, TInterface>, TInterface
		where TInterface : class, IQuery
	{
		/// <inheritdoc cref="IQuery.Conditionless"/>
		protected abstract bool Conditionless { get; }

		double? IQuery.Boost { get; set; }

		bool IQuery.Conditionless => Conditionless;

		bool IQuery.IsStrict { get; set; }

		bool IQuery.IsVerbatim { get; set; }

		bool IQuery.IsWritable => Self.IsVerbatim || !Self.Conditionless;
		string IQuery.Name { get; set; }

		/// <inheritdoc cref="IQuery.Name"/>
		public TDescriptor Name(string name) => Assign(name, (a, v) => a.Name = v);

		/// <inheritdoc cref="IQuery.Boost"/>
		public TDescriptor Boost(double? boost) => Assign(boost, (a, v) => a.Boost = v);

		/// <inheritdoc cref="IQuery.IsVerbatim"/>
		public TDescriptor Verbatim(bool verbatim = true) => Assign(verbatim, (a, v) => a.IsVerbatim = v);

		/// <inheritdoc cref="IQuery.IsStrict"/>
		public TDescriptor Strict(bool strict = true) => Assign(strict, (a, v) => a.IsStrict = v);
	}
}
