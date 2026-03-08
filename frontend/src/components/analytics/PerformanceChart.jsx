import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { formatNumber } from '../../utils/formatters';

export default function PerformanceChart({ data = [], dataKey = 'views', labelKey = 'title' }) {
  if (!data.length) {
    return (
      <div className="text-center py-5 text-muted">
        <i className="bi bi-bar-chart display-4"></i>
        <p className="mt-2">No data to display yet.</p>
      </div>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={350}>
      <BarChart data={data} margin={{ top: 10, right: 20, left: 0, bottom: 40 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#e9ecef" />
        <XAxis
          dataKey={labelKey}
          tick={{ fontSize: 12 }}
          angle={-35}
          textAnchor="end"
          interval={0}
          height={80}
        />
        <YAxis tick={{ fontSize: 12 }} tickFormatter={formatNumber} />
        <Tooltip
          formatter={(value) => [formatNumber(value), 'Views']}
          contentStyle={{ borderRadius: '8px', border: '1px solid #dee2e6' }}
        />
        <Bar dataKey={dataKey} fill="#0d6efd" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  );
}
