import React from 'react';
import PropTypes from 'prop-types';

import LinearProgress from 'material-ui/LinearProgress';
import FontIcon from 'material-ui/FontIcon';
import RaisedButton from 'material-ui/RaisedButton';

class SelectTag extends React.Component {
  selectTag(tagId) {
    this.props.selectTag(tagId);
  }

  render() {
    const style = {
      padding: 10,
      display: "flex",
      flexWrap: "wrap",
    };

    if (this.props.loading) {
      return (
        <div style={style}>
          <LinearProgress mode="indeterminate" />
        </div>
      );
    } else {
      const buttonStyle = {
        margin: 8
      };
      const iconStyle = {
        fontSize: 20
      }
      const labelStyle = {
        paddingLeft: 6
      }
      var buttons = this.props.tags.map(tag => (
        <RaisedButton
          onClick={this.selectTag.bind(this, tag.id)}
          label={tag.id}
          labelStyle={labelStyle}
          primary={true}
          style={buttonStyle}
          icon={
            <FontIcon
              className="material-icons"
              style={iconStyle}
            >
              add
            </FontIcon>}
        />
      ));
      return (
        <div style={style}>
          {buttons}
        </div>
      );
    }
  }
}

SelectTag.propTypes = {
  loading: PropTypes.bool,
  tags: PropTypes.array.isRequired,
  selectTag: PropTypes.func,
  theme: PropTypes.object
}

export default SelectTag;