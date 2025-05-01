import os
import pandas as pd
import numpy as np
from sklearn.linear_model import LinearRegression
import json

# Load CSV data
INPUT_CSV = "C:/Users/Ditchy/source/repos/BindicatorPython/ml-analysis/analysis_data.csv"
df_hist = pd.read_csv(INPUT_CSV, parse_dates=['Timestamp'])

# Configuration
MAX_WEIGHT = 25
MAX_FILL = 100

results = []

for bin_id, group in df_hist.groupby('BinNumber'):
    group = group.sort_values('Timestamp')
    group['days'] = (group['Timestamp'] - group['Timestamp'].min()).dt.days

    if len(group['days'].unique()) < 2:
        continue

    X = group[['days']].values
    y_weight = group['Weight'].values
    y_fill = group['FillLevel'].values

    weight_model = LinearRegression().fit(X, y_weight)
    fill_model = LinearRegression().fit(X, y_fill)

    current_weight = y_weight[-1]
    current_fill = y_fill[-1]

    weight_rate = weight_model.coef_[0]
    fill_rate = fill_model.coef_[0]

    days_to_full_weight = (MAX_WEIGHT - current_weight) / weight_rate if weight_rate > 0 else None
    days_to_full_fill = (MAX_FILL - current_fill) / fill_rate if fill_rate > 0 else None
    avg_days_to_full = np.mean([v for v in [days_to_full_weight, days_to_full_fill] if v is not None])

    if avg_days_to_full < 3:
        recommendation = "⚠️ Bin fills very quickly – suggest larger bin or more frequent collection."
    elif weight_rate < 0.2 and fill_rate < 0.3:
        recommendation = "🟢 Bin fills slowly – consider reducing collection frequency."
    else:
        recommendation = "✅ Bin fill rate is within expected range."

    result = {
        "bin_id": bin_id,
        "current_weight": round(current_weight, 2),
        "current_fill": round(current_fill, 2),
        "weight_rate_per_day": round(weight_rate, 2),
        "fill_rate_per_day": round(fill_rate, 2),
        "days_to_full_weight": round(days_to_full_weight, 1) if days_to_full_weight else None,
        "days_to_full_fill": round(days_to_full_fill, 1) if days_to_full_fill else None,
        "recommendation": recommendation
    }

    results.append(result)

# Output as JSON
json_output = json.dumps(results, indent=4)
print(json_output)

downloads_path = os.path.join(os.path.expanduser("~"), "Downloads")
output_path = os.path.join(downloads_path, "analysis_output.json")

with open(output_path, "w") as f:
    f.write(json_output)

print(f"✅ JSON saved to: {output_path}")
