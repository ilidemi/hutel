import React from 'react';
import PropTypes from 'prop-types';

import BubbleChart from './Chart/BubbleChart';

const Charts = (props) => { 
  if (!props.charts || !props.charts.length) {
    return null;
  }
  return (
    <div>
      {
        props.charts.map((chart, index) => {
          switch(chart.type) {
            case 'bubble': 
              return (
                <BubbleChart
                  chart={chart}
                  chartPoints={props.chartsPoints[index]}
                  key={index}
                />
              );
            default:
              console.error("Unknown chart type: " + chart.type);
          }
        })
      }
    </div>
  );
};

Charts.propTypes = {
  charts: PropTypes.array,
  chartsPoints: PropTypes.array
};

export default Charts;