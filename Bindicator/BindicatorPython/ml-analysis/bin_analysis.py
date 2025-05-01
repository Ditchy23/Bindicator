import os
import pandas as pd
import numpy as np
from sklearn.linear_model import LinearRegression
import json

# Load CSV data, amend path to match analysis_data.csv location
INPUT_CSV = "C:/Users/c2932952/Downloads/analysis_data.csv"
df_hist = pd.read_csv(INPUT_CSV, parse_dates=['Timestamp'])

# Configuration
MAX_WEIGHT = 25
MAX_FILL = 100

results = []

NEAR_FULL_THRESHOLD = 85  # percent considered "near full"

for bin_id, group in df_hist.groupby('BinNumber'):
    group = group.sort_values('Timestamp')
    group['days'] = (group['Timestamp'] - group['Timestamp'].min()).dt.days
    group['date'] = group['Timestamp'].dt.date  # Extract just the date

    if len(group['days'].unique()) < 2:
        continue  # Not enough data for a meaningful trend

    X = group[['days']].values
    y_fill = group['FillLevel'].values

    # Train regression model
    fill_model = LinearRegression().fit(X, y_fill)
    predicted_fill = fill_model.predict(X)
    predicted_fill_pct = (predicted_fill / MAX_FILL) * 100

    # Combine predictions with dates
    group['predicted_fill_pct'] = predicted_fill_pct

    # Aggregate by date (average predicted fill per day)
    daily_fill = group.groupby('date')['predicted_fill_pct'].mean()

    # Count days "near full"
    total_days = len(daily_fill)
    near_full_days = (daily_fill >= NEAR_FULL_THRESHOLD).sum()
    avg_fill_pct = daily_fill.mean()

    # Construct recommendation
    usage_statement = f"Bin has been near full for {near_full_days} out of {total_days} days monitored."
    if near_full_days / total_days >= 0.5:
        action = "⚠️ Consider a larger bin or more frequent collection."
    elif near_full_days == 0 and avg_fill_pct < 30:
        action = "ℹ️ Consider a smaller bin – current one is underused."
    else:
        action = "✅ Current bin size is appropriate."

    result = {
        "bin_id": bin_id,
        "average_fill_percent": round(avg_fill_pct, 2),
        "near_full_days": int(near_full_days),
        "total_days_monitored": int(total_days),
        "recommendation": f"{usage_statement} {action}"
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