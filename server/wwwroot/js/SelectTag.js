import React from 'react';
import PropTypes from 'prop-types';

class SelectTag extends React.Component {
  selectTag(tagId) {
    this.props.selectTag(tagId);
  }

  render() {
    var buttons = this.props.tags.map(tag => (
      <button onClick={this.selectTag.bind(this, tag.id)}>
        {tag.id}
      </button>)
    );
    return (
      <div>
        {buttons}
      </div>
    );
  }
}

SelectTag.propTypes = {
  tags: PropTypes.array.isRequired
}

export default SelectTag;