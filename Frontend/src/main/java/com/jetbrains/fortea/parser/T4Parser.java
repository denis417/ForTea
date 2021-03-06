// This is a generated file. Not intended for manual editing.
package com.jetbrains.fortea.parser;

import com.intellij.lang.PsiBuilder;
import com.intellij.lang.PsiBuilder.Marker;
import static com.jetbrains.fortea.psi.T4ElementTypes.*;
import static com.intellij.lang.parser.GeneratedParserUtilBase.*;
import com.intellij.psi.tree.IElementType;
import com.intellij.lang.ASTNode;
import com.intellij.psi.tree.TokenSet;
import com.intellij.lang.PsiParser;
import com.intellij.lang.LightPsiParser;

@SuppressWarnings({"SimplifiableIfStatement", "UnusedAssignment"})
public class T4Parser implements PsiParser, LightPsiParser {

  public ASTNode parse(IElementType root_, PsiBuilder builder_) {
    parseLight(root_, builder_);
    return builder_.getTreeBuilt();
  }

  public void parseLight(IElementType root_, PsiBuilder builder_) {
    boolean result_;
    builder_ = adapt_builder_(root_, builder_, this, null);
    Marker marker_ = enter_section_(builder_, 0, _COLLAPSE_, null);
    if (root_ == ATTRIBUTE) {
      result_ = attribute(builder_, 0);
    }
    else if (root_ == ATTRIBUTE_NAME) {
      result_ = attribute_name(builder_, 0);
    }
    else if (root_ == BLOCK) {
      result_ = block(builder_, 0);
    }
    else if (root_ == CODE_BLOCK) {
      result_ = code_block(builder_, 0);
    }
    else if (root_ == DIRECTIVE) {
      result_ = directive(builder_, 0);
    }
    else if (root_ == DIRECTIVE_NAME) {
      result_ = directive_name(builder_, 0);
    }
    else if (root_ == EXPRESSION_BLOCK) {
      result_ = expression_block(builder_, 0);
    }
    else if (root_ == FEATURE_BLOCK) {
      result_ = feature_block(builder_, 0);
    }
    else if (root_ == STATEMENT_BLOCK) {
      result_ = statement_block(builder_, 0);
    }
    else {
      result_ = parse_root_(root_, builder_, 0);
    }
    exit_section_(builder_, 0, marker_, root_, result_, true, TRUE_CONDITION);
  }

  protected boolean parse_root_(IElementType root_, PsiBuilder builder_, int level_) {
    return t4File(builder_, level_ + 1);
  }

  /* ********************************************************** */
  // attribute_name EQUAL QUOTE RAW_ATTRIBUTE_VALUE? QUOTE
  public static boolean attribute(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "attribute")) return false;
    if (!nextTokenIs(builder_, TOKEN)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = attribute_name(builder_, level_ + 1);
    result_ = result_ && consumeTokens(builder_, 0, EQUAL, QUOTE);
    result_ = result_ && attribute_3(builder_, level_ + 1);
    result_ = result_ && consumeToken(builder_, QUOTE);
    exit_section_(builder_, marker_, ATTRIBUTE, result_);
    return result_;
  }

  // RAW_ATTRIBUTE_VALUE?
  private static boolean attribute_3(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "attribute_3")) return false;
    consumeToken(builder_, RAW_ATTRIBUTE_VALUE);
    return true;
  }

  /* ********************************************************** */
  // TOKEN
  public static boolean attribute_name(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "attribute_name")) return false;
    if (!nextTokenIs(builder_, TOKEN)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = consumeToken(builder_, TOKEN);
    exit_section_(builder_, marker_, ATTRIBUTE_NAME, result_);
    return result_;
  }

  /* ********************************************************** */
  // directive | code_block
  public static boolean block(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "block")) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_, level_, _NONE_, BLOCK, "<block>");
    result_ = directive(builder_, level_ + 1);
    if (!result_) result_ = code_block(builder_, level_ + 1);
    exit_section_(builder_, level_, marker_, result_, false, null);
    return result_;
  }

  /* ********************************************************** */
  // statement_block|expression_block|feature_block
  public static boolean code_block(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "code_block")) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_, level_, _NONE_, CODE_BLOCK, "<code block>");
    result_ = statement_block(builder_, level_ + 1);
    if (!result_) result_ = expression_block(builder_, level_ + 1);
    if (!result_) result_ = feature_block(builder_, level_ + 1);
    exit_section_(builder_, level_, marker_, result_, false, null);
    return result_;
  }

  /* ********************************************************** */
  // directive_main BLOCK_END
  public static boolean directive(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "directive")) return false;
    if (!nextTokenIs(builder_, DIRECTIVE_START)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = directive_main(builder_, level_ + 1);
    result_ = result_ && consumeToken(builder_, BLOCK_END);
    exit_section_(builder_, marker_, DIRECTIVE, result_);
    return result_;
  }

  /* ********************************************************** */
  // DIRECTIVE_START directive_name? attribute*
  static boolean directive_main(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "directive_main")) return false;
    boolean result_, pinned_;
    Marker marker_ = enter_section_(builder_, level_, _NONE_);
    result_ = consumeToken(builder_, DIRECTIVE_START);
    pinned_ = result_; // pin = 1
    result_ = result_ && report_error_(builder_, directive_main_1(builder_, level_ + 1));
    result_ = pinned_ && directive_main_2(builder_, level_ + 1) && result_;
    exit_section_(builder_, level_, marker_, result_, pinned_, not_block_end_or_block_start_parser_);
    return result_ || pinned_;
  }

  // directive_name?
  private static boolean directive_main_1(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "directive_main_1")) return false;
    directive_name(builder_, level_ + 1);
    return true;
  }

  // attribute*
  private static boolean directive_main_2(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "directive_main_2")) return false;
    while (true) {
      int pos_ = current_position_(builder_);
      if (!attribute(builder_, level_ + 1)) break;
      if (!empty_element_parsed_guard_(builder_, "directive_main_2", pos_)) break;
    }
    return true;
  }

  /* ********************************************************** */
  // TOKEN
  public static boolean directive_name(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "directive_name")) return false;
    if (!nextTokenIs(builder_, TOKEN)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = consumeToken(builder_, TOKEN);
    exit_section_(builder_, marker_, DIRECTIVE_NAME, result_);
    return result_;
  }

  /* ********************************************************** */
  // EXPRESSION_BLOCK_START RAW_CODE? BLOCK_END
  public static boolean expression_block(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "expression_block")) return false;
    if (!nextTokenIs(builder_, EXPRESSION_BLOCK_START)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = consumeToken(builder_, EXPRESSION_BLOCK_START);
    result_ = result_ && expression_block_1(builder_, level_ + 1);
    result_ = result_ && consumeToken(builder_, BLOCK_END);
    exit_section_(builder_, marker_, EXPRESSION_BLOCK, result_);
    return result_;
  }

  // RAW_CODE?
  private static boolean expression_block_1(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "expression_block_1")) return false;
    consumeToken(builder_, RAW_CODE);
    return true;
  }

  /* ********************************************************** */
  // FEATURE_BLOCK_START RAW_CODE? BLOCK_END
  public static boolean feature_block(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "feature_block")) return false;
    if (!nextTokenIs(builder_, FEATURE_BLOCK_START)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = consumeToken(builder_, FEATURE_BLOCK_START);
    result_ = result_ && feature_block_1(builder_, level_ + 1);
    result_ = result_ && consumeToken(builder_, BLOCK_END);
    exit_section_(builder_, marker_, FEATURE_BLOCK, result_);
    return result_;
  }

  // RAW_CODE?
  private static boolean feature_block_1(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "feature_block_1")) return false;
    consumeToken(builder_, RAW_CODE);
    return true;
  }

  /* ********************************************************** */
  // !BLOCK_END
  static boolean not_block_end_or_block_start(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "not_block_end_or_block_start")) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_, level_, _NOT_);
    result_ = !consumeToken(builder_, BLOCK_END);
    exit_section_(builder_, level_, marker_, result_, false, null);
    return result_;
  }

  /* ********************************************************** */
  // STATEMENT_BLOCK_START RAW_CODE? BLOCK_END
  public static boolean statement_block(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "statement_block")) return false;
    if (!nextTokenIs(builder_, STATEMENT_BLOCK_START)) return false;
    boolean result_;
    Marker marker_ = enter_section_(builder_);
    result_ = consumeToken(builder_, STATEMENT_BLOCK_START);
    result_ = result_ && statement_block_1(builder_, level_ + 1);
    result_ = result_ && consumeToken(builder_, BLOCK_END);
    exit_section_(builder_, marker_, STATEMENT_BLOCK, result_);
    return result_;
  }

  // RAW_CODE?
  private static boolean statement_block_1(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "statement_block_1")) return false;
    consumeToken(builder_, RAW_CODE);
    return true;
  }

  /* ********************************************************** */
  // (RAW_TEXT|NEW_LINE|block)*
  static boolean t4File(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "t4File")) return false;
    while (true) {
      int pos_ = current_position_(builder_);
      if (!t4File_0(builder_, level_ + 1)) break;
      if (!empty_element_parsed_guard_(builder_, "t4File", pos_)) break;
    }
    return true;
  }

  // RAW_TEXT|NEW_LINE|block
  private static boolean t4File_0(PsiBuilder builder_, int level_) {
    if (!recursion_guard_(builder_, level_, "t4File_0")) return false;
    boolean result_;
    result_ = consumeToken(builder_, RAW_TEXT);
    if (!result_) result_ = consumeToken(builder_, NEW_LINE);
    if (!result_) result_ = block(builder_, level_ + 1);
    return result_;
  }

  final static Parser not_block_end_or_block_start_parser_ = new Parser() {
    public boolean parse(PsiBuilder builder_, int level_) {
      return not_block_end_or_block_start(builder_, level_ + 1);
    }
  };
}
